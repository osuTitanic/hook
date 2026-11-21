using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Helpers;

namespace TitanicHook.Core.Hooks.Misc;

/// <summary>
/// Removes the client-side BSS file size check, allowing the server to validate the size on its own.
/// </summary>
public class RemoveBssSizeCheck : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.RemoveBssSizeCheck";

    public RemoveBssSizeCheck() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Transpilers = [AccessTools.Method(typeof(RemoveBssSizeCheck), nameof(Transpiler))];
    }

    private static MethodInfo? GetTargetMethod()
    {
        MethodInfo? method = AssemblyUtils.OsuTypes
            .Where(t => t is { IsClass: true, IsNested: false, IsNotPublic: true } && t.BaseType != typeof(object))
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            .FirstOrDefault(m => m.ReturnType.FullName == "System.Void" &&
                                 m.GetParameters().Length == 2 &&
                                 m.GetParameters()[1].ParameterType.Name == "DoWorkEventArgs" &&
                                 SigScanning.GetStrings(m)
                                     .Any(s => s.Contains("beatmap is too large") || s == "Marathon"));

        return method;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> final = new(instructions);
        for (int i = 0; i < final.Count; i++)
        {
            var instr = final[i];

            // Here we try to find the check for when osu! checks for the filesize of the beatmap and jumps to the less or equal branch.
            // We remove the check and make the code jump to the success instruction directly.
            if (instr.opcode != OpCodes.Ldfld) continue;
            if (final[i - 1].opcode != OpCodes.Ldarg_0) continue;
            if (final[i + 1].opcode != OpCodes.Ldc_I4) continue;
            if (final[i + 3].opcode != OpCodes.Ble) continue; // ble.s in disassembly

            var jumpInstr = final[i + 3];
            instr.opcode = OpCodes.Br; // would be br if it was ble.s
            instr.operand = jumpInstr.operand;

            final[i - 1].opcode = OpCodes.Nop;
            final[i + 2].opcode = OpCodes.Nop;
            final[i + 3].opcode = OpCodes.Nop;
        }

        return final;
    }
}
