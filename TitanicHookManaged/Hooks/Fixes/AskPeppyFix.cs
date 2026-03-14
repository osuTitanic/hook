using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Fixes;

/// <summary>
/// Fixes the annoying "do you really want to ask peppy" redirect
/// when trying to message a person with ID 2.
/// </summary>
public class AskPeppyFix : TitanicPatch
{
    private static readonly string HookName = "sh.Titanic.Hook.AskPeppyFix";

    public AskPeppyFix() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Transpilers = [AccessTools.Method(typeof(AskPeppyFix), nameof(Transpiler))];
    }

    private static MethodInfo? GetTargetMethod()
    {
        MethodInfo? method = AssemblyUtils.OsuTypes
            .Where(t => t.IsNotPublic && !t.IsNested && t.BaseType != null && t.BaseType != typeof(object))
            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            .FirstOrDefault(
                m => m.ReturnType.FullName == "System.Void" &&
                     m.GetParameters().Length == 1 &&
                     SigScanning.GetStrings(m).Any(s => s.Contains("osu.ppy.sh/p/doyoureallywanttoaskpeppy")));

        if (method == null)
        {
            Logging.HookError(HookName, "Failed to find target method", EntryPoint.Config.FirstRun);
            if (EntryPoint.Config.FirstRun)
                EntryPoint.Config.RemovePeppyDmCheck = false;
            return null;
        }

        return method;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> final = new (ObfHelper.DecAllStrings(instructions));
        for (int i = 0; i < final.Count; i++)
        {
            CodeInstruction instruction = final[i];
            if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string s &&
                s.Contains("osu.ppy.sh/p/doyoureallywanttoaskpeppy"))
            {
                instruction.opcode = OpCodes.Nop; // ldstr
                final[i + 1].opcode = OpCodes.Nop; // ldnull
                final[i + 2].opcode = OpCodes.Nop; // call
                final[i + 3].opcode = OpCodes.Nop; // ret
            }
        }

        foreach (CodeInstruction instruction in final)
        {
            Logging.Info(instruction.ToString());
        }
        return final.AsEnumerable();
    }
}
