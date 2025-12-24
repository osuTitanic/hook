// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;

namespace TitanicHookManaged.Helpers;

/// <summary>
/// Some utils relating to obfuscation
/// </summary>
public static class ObfHelper
{
    /// <summary>
    /// Prefix signature for SmartAssembly 3.x string decrypt method, currently unused
    /// </summary>
    private static OpCode[] _stringObfPrefix =
    [
        OpCodes.Ldtoken,
        OpCodes.Call,
        OpCodes.Dup,
        OpCodes.Stloc_S,
        OpCodes.Call,
        OpCodes.Ldsfld,
        OpCodes.Brtrue,
        OpCodes.Call,
        OpCodes.Stloc_1,
        OpCodes.Ldloc_1,
        OpCodes.Ldloc_1,
        OpCodes.Callvirt,
        OpCodes.Callvirt,
        OpCodes.Stloc_S,
        OpCodes.Ldloca_S,
        OpCodes.Ldstr,
        OpCodes.Call,
        OpCodes.Callvirt,
        OpCodes.Stsfld,
    ];
    
    /// <summary>
    /// Reference to SmartAssembly string decrypt method
    /// </summary>
    static MethodInfo? _saStringObfReference = AssemblyUtils.OsuTypes
        .Where(t => t.IsClass &&
                    t.IsSealed &&
                    t.IsNotPublic && 
                    !t.IsNested &&
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public).Length == 1
                    )
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
        .FirstOrDefault(m => m.GetParameters().Length == 1 && 
                             m.GetParameters()[0].ParameterType.FullName == "System.Int32" && 
                             m.ReturnType.FullName == "System.String" /*&&*/ 
                             /*SigScanning.GetOpcodes(m).StartsWith(_stringObfPrefix)*/); // TODO: control flow obfuscation in that method adds Br opcodes, this is not handled yet but for now the lookup works good enough

    /// <summary>
    /// Reference to Eazfuscator string decrypt method
    /// </summary>
    private static MethodInfo? _eazStringObfReference = AssemblyUtils.OsuTypes
        .Where(t => t.IsClass &&
                    t.IsAbstract &&
                    t.IsSealed &&
                    t.IsNotPublic &&
                    !t.IsNested)
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
        .FirstOrDefault(m => m.GetParameters().Length == 1 &&
                             m.GetParameters()[0].ParameterType.FullName == "System.Int32" &&
                             m.ReturnType.FullName == "System.String" /*&&*/
                             /*SigScanning.MethodHasNoInlining(m)*/); // TODO: fucked attribute check
    
    /// <summary>
    /// Reference to the first found string decryption method
    /// </summary>
    private static MethodInfo? _stringObfReference = _saStringObfReference ?? _eazStringObfReference;
    
    public static bool HasStringDecrypt => _stringObfReference != null;
    public static int StringObfToken => _stringObfReference?.MetadataToken ?? 0;

    /// <summary>
    /// Decrypts an obfuscated string
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string? DecString(int id)
    {
        // Try to get from cache
        if (_stringCache.TryGetValue(id, out var value))
            return value;

        if (_stringObfReference == null)
            return null;

        if (_decStringDelegate == null)
        {
            // Create a delegate to quickly call string decryption without reflection invoking
            var dm = new DynamicMethod(
                "Invoke_StringDec", 
                typeof(string), 
                [typeof(int)], 
                _stringObfReference.DeclaringType.Module, 
                true);
            
            var il = dm.GetILGenerator();
            
            il.Emit(OpCodes.Ldarg_0); // Load the int argument
            il.Emit(OpCodes.Call, _stringObfReference); // Call the string decryption method
            il.Emit(OpCodes.Box, typeof(string)); // Box the method into a string
            il.Emit(OpCodes.Ret); // Return the decrypted string
            
            // Create the delegate
            _decStringDelegate = (DecStringDelegate) dm.CreateDelegate(typeof(DecStringDelegate));
        }

        // It's not in cache, so call the deobfuscation method and add it to cache
        value = _decStringDelegate(id) ?? "";
        _stringCache.Add(id, value);
        
        return value;
    }
    
    private static DecStringDelegate? _decStringDelegate;
    private delegate string? DecStringDelegate(int id);

    /// <summary>
    /// Decrypts all encrypted strings in a specified CodeInstruction enumerable
    /// </summary>
    /// <param name="instructions"></param>
    /// <returns></returns>
    public static IEnumerable<CodeInstruction> DecAllStrings(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new (instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            var instr = codes[i];

            if (HasStringDecrypt && instr.opcode == OpCodes.Call &&
                instr.operand is MethodInfo methodInfo &&
                methodInfo.MetadataToken == StringObfToken)
            {
                var stringIdInstr = codes[i - 1];
                if (stringIdInstr.opcode != OpCodes.Ldc_I4)
                {
                    // The instruction above is not a local load, so the function is most likely a branch
                    // target, so we have to find the instruction that branches into the call and get the
                    // argument from above that (this caused issues in b20140818.8 when developing AskPeppyFix)
                    stringIdInstr = codes[FindArgLoadInBranchTarget(codes, instr.labels[0])]; // Let's hope that it's always the 0 index
                }
                
                int stringId = (int)stringIdInstr.operand;
                // Logging.Info($"{stringIdInstr.opcode} {stringIdInstr.operand} @ {instr.operand} {instr.operand} (i={i})");
                // Debug.Assert(stringIdInstr.opcode == OpCodes.Ldc_I4);

                string? deobfuscatedString = DecString(stringId);
                if (deobfuscatedString == null)
                    continue;
                
                // Replace with decoded string
                instr.opcode = OpCodes.Ldstr;
                instr.operand = deobfuscatedString;
                
                // Remove the argument load
                stringIdInstr.opcode = OpCodes.Nop;
                stringIdInstr.operand = null;
            }
        }
        
        return codes.AsEnumerable();
    }

    /// <summary>
    /// Tries to find an index of an integer load for a method, where call and arg load are separated by an Br(.s).
    /// Note: designed originally for Br.s, but sometimes the JIT turns Br.s into Br
    ///
    /// Example:
    /// ldc.i4 0xDEADBEEF
    /// br.s IL_0010
    /// ...
    /// IL_0100:
    /// call somefunction(Int32)
    /// </summary>
    /// <param name="instructions">List of Harmony instructions</param>
    /// <param name="instructionLabel">Label of the call instruction</param>
    /// <returns>The location of the integer load in the instructions list</returns>
    /// <exception cref="Exception">The argument load couldn't be found</exception>
    private static int FindArgLoadInBranchTarget(List<CodeInstruction> instructions, Label instructionLabel)
    {
        for (int i = 0; i < instructions.Count; i++)
        {
            var instr = instructions[i];
            if (!((instr.opcode == OpCodes.Br_S || instr.opcode == OpCodes.Br) && instr.operand is Label target && target == instructionLabel))
                continue;
            
            try
            {
                // small sanity check if we got the right thing
                var instrAbove = instructions[i - 1];
                int operand = (int)instrAbove.operand;
            }
            catch (Exception e)
            {
                // Probably non-fatal and probably won't happen ever but worth logging anyways
                Logging.Info($"Instruction above's operand wasn't an int despite finding a branch instrucution with correct label");
                continue;
            }
            
            return i - 1;
        }
        
        throw new Exception("Could not find arg load instruction");
    }
    
    /// <summary>
    /// Cache for deobfuscated strings
    /// </summary>
    private static Dictionary<int, string> _stringCache = new ();
}
