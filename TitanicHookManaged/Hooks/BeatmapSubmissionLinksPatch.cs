using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public class BeatmapSubmissionLinksPatch : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.BeatmapSubmissionLinksPatch";

    public BeatmapSubmissionLinksPatch() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Transpilers = [AccessTools.Method(typeof(BeatmapSubmissionLinksPatch), nameof(LinksTranspiler))];
    }

    private static MethodInfo? GetTargetMethod()
    {
        return AssemblyUtils.OsuTypes
            .Where(t => t is { IsClass: true, IsNested: false, IsNotPublic: true } && t.BaseType != typeof(object))
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            .FirstOrDefault(m => m.ReturnType.FullName == "System.Void" &&
                                 m.GetParameters().Length is 0 or 2 &&
                                 SigScanning.GetStrings(m)
                                     .Any(s => s.Contains("This beatmap was submitted using in-game submission on"))
            );
    }
    
    #region Hook

    private static IEnumerable<CodeInstruction> LinksTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in ObfHelper.DecAllStrings(instructions))
        {
            if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string str && (str.Contains("ppy.sh") || str.Contains("peppy.chigau.com")))
            {
                Logging.HookStep(HookName, $"Patching string {str}");
                string newstr = str.Replace("ppy.sh", EntryPoint.Config.ServerName);
                newstr = newstr.Replace("peppy.chigau.com", $"chigau.{EntryPoint.Config.ServerName}");
                yield return new CodeInstruction(OpCodes.Ldstr, newstr);
                continue;
            }
            
            yield return instruction;
        }
    }
    
    #endregion
}
