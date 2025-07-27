using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;

namespace TitanicHookManaged.Helpers;

public class SigScanning
{
    /// <summary>
    /// Gets all OpCodes from a method
    /// </summary>
    /// <param name="method">Method</param>
    /// <returns>All OpCodes</returns>
    public static OpCode[] GetOpcodes(MethodInfo method)
    {
        // TODO: Fix this for builds using SmartAssembly control flow obfuscation.
        // SmartAssembly seems to add Br.s opcodes to confuse IL readers
        ILReader reader = new ILReader(method);
        return reader.Select(instr => instr.OpCode)
            .ToArray();
    }
    
    public static OpCode[] GetOpcodes2(MethodInfo method)
    {
        ILReader reader = new ILReader(method);
        ILInstruction[] instructions = reader.ToArray();
        Dictionary<int, ILInstruction> offsetAndInstr = instructions.ToDictionary(instr => instr.Offset);
        // List<int> visitedOffsets = [];
        // List<ILInstruction> finalInstructions = [];
        HashSetCompat<int> cleanOffsets = new();
        List<OpCode> cleanOpcodes = [];
        foreach (KeyValuePair<int, ILInstruction> instr in offsetAndInstr)
        {
            // if (instr.Value is ShortInlineBrTargetInstruction jump)
            //     cleanOffsets.Add(jump.TargetOffset);
            // else
            //     cleanOffsets.Add(instr.Value.Offset);
            cleanOffsets.Add(GetRealOffset(instr.Key, offsetAndInstr));
        }

        foreach (int offset in cleanOffsets)
        {
             cleanOpcodes.Add(offsetAndInstr[offset].OpCode);
        }
        
        return cleanOpcodes.ToArray();
    }

    private static int GetRealOffset(int offset, Dictionary<int, ILInstruction> offsetAndInstr)
    {
        ILInstruction instr = offsetAndInstr[offset];
        while (true)
        {
            if (instr.OpCode != OpCodes.Br_S)
                return instr.Offset;
            
            ShortInlineBrTargetInstruction jump = (ShortInlineBrTargetInstruction)instr;
            
            instr = offsetAndInstr[jump.TargetOffset];
        }
    }
}
