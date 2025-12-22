// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Linq;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Misc;

/// <summary>
/// Patch which removes score loading delay in clients that use /web/osu-osz2-getscores.php endpoint
/// </summary>
public class RemoveScoreDelayHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.RemoveScoreDelay";

    public RemoveScoreDelayHook() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Prefixes = [AccessTools.Method(typeof(RemoveScoreDelayHook), nameof(GetScoresPrefix))];
    }

    private static MethodInfo? GetTargetMethod()
    {
        // TODO: Might want to add some stricter class checks
        MethodInfo? targetMethod = AssemblyUtils.OsuTypes
            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
            .FirstOrDefault(m => m.ReturnType.FullName == "System.Void" &&
                                 m.GetParameters().Length >= 2 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.Boolean" &&
                                 m.GetParameters()[1].ParameterType.FullName == "System.Boolean" &&
                                 SigScanning.GetStrings(m).Any(s => s.Contains("/web/osu-osz2-getscores.php?s=")));

        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Couldn't find target method", !EntryPoint.Config.FirstRun);
            if (EntryPoint.Config.FirstRun)
                EntryPoint.Config.RemoveScoreFetchingDelay = false;
            return null;
        }
        
        return targetMethod;
    }

    #region Hook

    // Set force to true
    private static void GetScoresPrefix(ref bool __1)
    {
        Logging.HookTrigger(HookName);
        __1 = true;
    }

    #endregion
}
