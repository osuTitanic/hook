using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        List<CodeInstruction> codes = new (instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            var instruction = codes[i];

            if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string str && (str.Contains("ppy.sh") || str.Contains("peppy.chigau.com")))
            {
                // Unobfuscated string, can just easily replace the string
                Logging.HookStep(HookName, $"Patching string {str}");
                string newstr = str.Replace("ppy.sh", EntryPoint.Config.ServerName);
                newstr = newstr.Replace("peppy.chigau.com", $"chigau.{EntryPoint.Config.ServerName}");
                codes[i] = new CodeInstruction(OpCodes.Ldstr, newstr);
                continue;
            }

            if (ObfHelper.HasStringDecrypt && instruction.opcode == OpCodes.Call &&
                instruction.operand is MethodInfo methodInfo &&
                methodInfo.MetadataToken == ObfHelper.StringObfToken)
            {
                // Here we have an obfuscated string - we need to find out if it contains "ppy.sh" by deobfuscating it
                var stringIdInstr = codes[i - 1];
                int stringId = (int)stringIdInstr.operand;

                string? deobfuscatedString = ObfHelper.DecString(stringId);
                if (deobfuscatedString == null)
                    continue;
                
                if (!deobfuscatedString.Contains("ppy.sh") && !deobfuscatedString.Contains("peppy.chigau.com"))
                    continue;
                
                Logging.HookStep(HookName, $"Patching string {deobfuscatedString}");
                string newstr = deobfuscatedString.Replace("ppy.sh", EntryPoint.Config.ServerName);
                newstr = newstr.Replace("peppy.chigau.com", $"chigau.{EntryPoint.Config.ServerName}");
                
                codes[i] = new CodeInstruction(OpCodes.Ldstr, newstr); // Replace obfuscated string call with a normal ldstr
                codes[i - 1] = new CodeInstruction(OpCodes.Nop); // Remove the argument load
            }
        }
        
        return codes.AsEnumerable();
    }
    
    #endregion
}
