using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public static class BeatmapSubmissionLinksPatch
{
    public const string HookName = "sh.Titanic.Hook.BeatmapSubmissionLinksPatch";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);

        MethodInfo? targetMethod = AssemblyUtils.OsuTypes
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            .FirstOrDefault(m => SigScanning.GetStrings(m)
                .Any(s => s.Contains("This beatmap was submitted using in-game submission on"))
            );

        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Failed to find Beatmap SubmissionPostMethod");
            return;
        }

        var transpiler = typeof(BeatmapSubmissionLinksPatch).GetMethod("LinksTranspiler", Constants.HookBindingFlags);

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, transpiler: new HarmonyMethod(transpiler));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
    }
    
    #region Hook

    private static IEnumerable<CodeInstruction> LinksTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new (instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            var instruction = codes[i];

            if (instruction.opcode == OpCodes.Ldstr && instruction.operand is string str && str.Contains("ppy.sh"))
            {
                // Unobfuscated string, can just easily replace the string
                Logging.HookStep(HookName, $"Patching string {str}");
                string newstr = str.Replace("ppy.sh", EntryPoint.Config.ServerName);
                codes[i] = new CodeInstruction(OpCodes.Ldstr, newstr);
                continue;
            }

            if (ObfHelper.HasStringDecrypt && instruction.opcode == OpCodes.Call &&
                instruction.operand is MethodInfo methodInfo &&
                methodInfo.MetadataToken == ObfHelper.StringObfToken)
            {
                // Here we have an obfuscated string - we need to find out if it contains "ppy.sh" by deobfuscating it
                var stringIdInstr = codes[i - 1];
                if (stringIdInstr.operand is not int stringId)
                    continue;

                string? deobfuscatedString = ObfHelper.DecString(stringId);
                if (deobfuscatedString == null)
                    continue;
                
                if (!deobfuscatedString.Contains("ppy.sh"))
                    continue;
                
                Logging.HookStep(HookName, $"Patching string {deobfuscatedString}");
                string newstr = deobfuscatedString.Replace("ppy.sh", EntryPoint.Config.ServerName);
                
                codes[i] = new CodeInstruction(OpCodes.Ldstr, newstr); // Replace obfuscated string call with a normal ldstr
                codes[i - 1] = new CodeInstruction(OpCodes.Nop); // Remove the argument load
            }
        }
        
        return codes.AsEnumerable();
    }
    
    #endregion
}
