// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Collections.Generic;
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

        // It's not in cache, so call the deobfuscation method and add it to cache
        value = _stringObfReference?.Invoke(null, [id])?.ToString() ?? "";
        _stringCache.Add(id, value);
        
        return value;
    }

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
                int stringId = (int)stringIdInstr.operand;

                string? deobfuscatedString = DecString(stringId);
                if (deobfuscatedString == null)
                    continue;
                
                codes[i].operand = deobfuscatedString; // Replace with decoded string
                codes[i - 1].opcode = OpCodes.Nop; // Remove the argument load
            }
        }
        
        return codes.AsEnumerable();
    }
    
    /// <summary>
    /// Cache for deobfuscated strings
    /// </summary>
    private static Dictionary<int, string> _stringCache = new ();
}
