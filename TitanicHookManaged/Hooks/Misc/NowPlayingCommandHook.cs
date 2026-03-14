// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Misc;

/// <summary>
/// Hook which will replace ppy.sh links to server link in /np command
/// </summary>
public class NowPlayingCommandHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.NowPlayingCommandHook";

    public NowPlayingCommandHook() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Prefixes = [AccessTools.Method(typeof(NowPlayingCommandHook), nameof(SendMessagePrefix))];
    }

    #region Hook
    
    private static void SendMessagePrefix(ref string __0)
    {
        Logging.HookTrigger(HookName);
        Logging.HookOutput(HookName, $"Message: {__0}");
        string[] prefixes = ["/me is listening", "/me is watching", "/me is playing"];
        if (prefixes.Any(__0.StartsWith))
            __0 = Regex.Replace(__0, @"(?<=https?:\/\/osu\.)ppy\.sh", EntryPoint.Config.ServerName);
    }

    #endregion

    #region Find method

    private static MethodInfo? GetTargetMethod()
    {
        return AssemblyUtils.OsuTypes
            .Where(t => t.IsClass && t.IsNotPublic && t.BaseType != typeof(object))
            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
            .FirstOrDefault(m => m.GetParameters().Length is 2 or 3 && 
                                 m.GetParameters()[0].ParameterType.FullName == "System.String" && 
                                 m.GetParameters()[1].ParameterType.FullName is "System.String" or "System.Boolean" &&
                                 SigScanning.GetStrings(m).Contains("/bb "));
    }

    #endregion
}
