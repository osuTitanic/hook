using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;

namespace TitanicHookManaged.Helpers;

public static class SigScanning
{
    public static bool StartsWith<T>(this T[] source, T[] prefix)
    {
        if (prefix.Length > source.Length)
            return false;
        
        return source.Take(prefix.Length).SequenceEqual(prefix);
    }

    /// <summary>
    /// Gets all strings from a method
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static string[] GetStrings(MethodInfo method)
    {
        List<string> strings = [];
        ILReader reader = new ILReader(method);
        ILInstruction[] instructions = reader.ToArray();
        for (int i = 0; i < instructions.Length; i++)
        {
            ILInstruction instruction = instructions[i];

            // Normal, unobfuscated string
            if (instruction is InlineStringInstruction strInstr)
            {
                strings.Add(strInstr.String);
                continue;
            }
            
            // Maybe obfuscated string? (calling the string decryption method)
            if (ObfHelper.HasStringDecrypt && instruction is InlineMethodInstruction callInstr &&
                callInstr.Token == ObfHelper.StringObfToken)
            {
                // Get argument for string decrypt method
                if (instructions[i - 1] is not InlineIInstruction loadArg) continue;

                int arg = loadArg.Int32;
                strings.Add(ObfHelper.DecString(arg));
            }
        }
        return strings.ToArray();
    }
    
    /// <summary>
    /// Compare method against multiple exact signatures
    /// </summary>
    /// <param name="m">Method</param>
    /// <param name="signatures">List of OpCode signatures</param>
    /// <returns>At least one signature was matching</returns>
    public static bool CompareMultipleSigs(MethodInfo m, OpCode[][] signatures)
    {
        OpCode[] methodOpcodes = GetOpcodes(m);
        foreach (OpCode[] signature in signatures)
        {
            if (methodOpcodes.SequenceEqual(signature)) return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Get IL OpCodes of a method, deobfuscating Br.S statements in the process
    /// </summary>
    /// <param name="method">Target method</param>
    /// <returns>IL opcodes of the method</returns>
    public static OpCode[] GetOpcodes(MethodInfo method)
    {
        // TODO: Might want to add a variant when control flow obfuscation is not used.
        // Running the deobfuscation code on unobfuscated executables is probably not the best.
        // Original code for unobfuscated methods:
        // ILReader reader = new ILReader(method);
        // return reader.Select(instr => instr.OpCode)
        //    .ToArray();
        
        // Get an Offset:ILInstruction map of the method
        Dictionary<int, ILInstruction> offsetAndInstr = new ILReader(method)
            .ToArray()
            .ToDictionary(instr => instr.Offset);
        
        HashSetCompat<int> visitedOffsets = new(); // Store unique visited offsets
        HashSetCompat<int> cleanOffsets = new(); // Store offsets containing actual offsets (not jumps)
        List<OpCode> cleanOpcodes = []; // Store the clean OpCodes (without jumps)
        
        // Iterate over all instructions here
        foreach (KeyValuePair<int, ILInstruction> instr in offsetAndInstr)
        {
            // Skip already visited offsets
            if (visitedOffsets.Contains(instr.Key))
                continue;
            
            // Get actual instruction offset, add it to clean offsets
            cleanOffsets.Add(GetRealOffset(instr.Key, offsetAndInstr, ref visitedOffsets));
            
            // Do not add this offset here as it was already added by GetRealOffset
        }

        // Get all OpCodes from the clean offsets
        foreach (int offset in cleanOffsets)
        {
             cleanOpcodes.Add(offsetAndInstr[offset].OpCode);
        }
        
        return cleanOpcodes.ToArray();
    }

    /// <summary>
    /// Resolves all Br.S statements used in the process of control flow obfuscation
    /// </summary>
    /// <param name="offset">Offset of the current method</param>
    /// <param name="offsetAndInstr">Offset:Instruction mapping</param>
    /// <param name="visitedOffsets">Reference to visitedOffsets HashSet</param>
    /// <returns>Offset of actual IL instruction</returns>
    private static int GetRealOffset(int offset, Dictionary<int, ILInstruction> offsetAndInstr, ref HashSetCompat<int> visitedOffsets)
    {
        ILInstruction instr = offsetAndInstr[offset];
        while (true)
        {
            visitedOffsets.Add(instr.Offset);
            
            if (instr.OpCode != OpCodes.Br_S)
                return instr.Offset;
            
            ShortInlineBrTargetInstruction jump = (ShortInlineBrTargetInstruction)instr;
            
            instr = offsetAndInstr[jump.TargetOffset];
        }
    }
}
