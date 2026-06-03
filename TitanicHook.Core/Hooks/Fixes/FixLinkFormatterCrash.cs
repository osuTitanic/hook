using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Helpers;

namespace TitanicHook.Core.Hooks.Fixes;

/// <summary>
/// Fix a crash that happens when opening chat in 2009-2012 clients on .NET Framework 4.
/// In some cases osu! can pass in a value smaller than 0 as startIndex param.
/// This was in an undefined behavior area in .NET Framework 2.0 and did not raise an exception, however
/// in .NET Framework 4 this behavior is forbidden and an exception is raised if startIndex is smaller than 0.
/// </summary>
public class FixLinkFormatterCrash : TitanicPatch
{
    private static readonly string HookName = "sh.Titanic.Hook.FixLinkFormatterCrash";
    public override PatchImportance Importance => PatchImportance.None;
    
    public FixLinkFormatterCrash() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Transpilers = [AccessTools.Method(typeof(FixLinkFormatterCrash), nameof(Transpiler))];
    }

    private MethodInfo? GetTargetMethod()
    {
        MethodInfo? targetMethod = AssemblyUtils.OsuTypes
            .Where(t => t.IsNotPublic && !t.IsNested && t.BaseType != null && t.BaseType != typeof(object))
            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            .FirstOrDefault(m => m.ReturnType == typeof(void) && m.GetParameters().Length == 0 && SigScanning.GetStrings(m).Contains("osump://"));

        return targetMethod;
    }

    private static IEnumerable<CodeInstruction>? Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> final = new (ObfHelper.DecAllStrings(instructions));
        for (int i = 0; i < final.Count; i++)
        {
            var instr = final[i];
            /*if (instr.opcode == OpCodes.Ldstr && instr.operand is string s && s == ")[")
            {
                // Patch that call with our patched IndexOf
                var indexOfCall = final[i + 4];
                Debug.Assert(indexOfCall.opcode == OpCodes.Callvirt && indexOfCall.operand is MethodInfo mi && mi.Name == "IndexOf");
                
                indexOfCall.opcode = OpCodes.Call;
                indexOfCall.operand = AccessTools.Method(typeof(FixLinkFormatterCrash), nameof(PatchedIndexOf));
            }*/
            if (instr.opcode == OpCodes.Callvirt && instr.operand is MethodInfo method && method.Name == "IndexOf" &&
                method.GetParameters().Length == 2)
            {
                // Patch that call with our patched IndexOf
                instr.opcode = OpCodes.Call;
                instr.operand = AccessTools.Method(typeof(FixLinkFormatterCrash), nameof(PatchedIndexOf));
            }
        }
        
        return final;
    }

    private static int PatchedIndexOf(string thisstr, string value, int startIndex)
    {
        // Empty strings are invalid
        if (thisstr == "" || startIndex == 0)
            return -1;
        
        // Clamp startIndex to a safe value
        /*if (startIndex < 0)
            startIndex = 0;*/
        
        // ReSharper disable StringIndexOfIsCultureSpecific.2
        return thisstr.IndexOf(value, startIndex);
        // ReSharper restore StringIndexOfIsCultureSpecific.2
    }
}
