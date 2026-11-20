using Harmony;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Hooks.Fixes;

/// <summary>
/// Allows CompareInfo.IndexOf to accept negative startIndex provided that it's done when osu! is doing chat link formatting
/// </summary>
public class AllowNegativeIndexOfStarts : TitanicPatch
{
    private static readonly string HookName = "sh.Titanic.Hook.AllowNegativeIndexOfStarts";
    public override PatchImportance Importance => PatchImportance.None;

    public AllowNegativeIndexOfStarts(): base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Transpilers = [AccessTools.Method(typeof(AllowNegativeIndexOfStarts), nameof(Transplier))];
    }

    private static MethodInfo? GetTargetMethod()
    {
        return typeof(CompareInfo)
            .GetMethods()
            .Where(m => m.Name == "IndexOf" && m.GetParameters().Length == 6 && m.GetParameters()[1].ParameterType.FullName == "System.String")
            .FirstOrDefault();
    }

    private static IEnumerable<CodeInstruction> Transplier(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> final = new(instructions);
        for (int i = 0; i < final.Count; i++)
        {
            var instr = final[i];
            if (instr.opcode == OpCodes.Ldstr && instr.operand is string s && s == "ArgumentOutOfRange_Index")
            {
                // Patch the place where OutOfRange exception would be thrown to do a check for osu!
                final[i - 1].opcode = OpCodes.Call;
                final[i - 1].operand = AccessTools.Method(typeof(AllowNegativeIndexOfStarts), nameof(ThrowIfNotFromOsu));
                //final[i - 1].opcode = OpCodes.Nop;

                final[i].opcode = OpCodes.Nop;
                final[i + 1].opcode = OpCodes.Nop;
                final[i + 2].opcode = OpCodes.Nop;
                final[i + 3].opcode = OpCodes.Nop;
            }
        }

        return final;
    }

    private static void ThrowIfNotFromOsu()
    {
        // Do nothing if it's osu! doing chat formatting
        if (FixLinkFormatterCrash.DoingChatFormatting) return;

        // It's not osu! passing weird args
        throw new ArgumentOutOfRangeException("startIndex", "ArgumentOutOfRange_Index");
    }
}
