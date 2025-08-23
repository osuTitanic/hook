// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook which will replace ppy.sh links to server link in /np command
/// </summary>
public static class NowPlayingCommandHook
{
    public const string HookName = "sh.Titanic.Hook.NowPlayingCommandHook";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);
        
        MethodInfo? targetMethod = GetTargetMethod();
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Couldn't find target method");
            return;
        }
        
        var prefix = typeof(NowPlayingCommandHook).GetMethod("SendMessagePrefix", Constants.HookBindingFlags);

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
            return;
        }
        Logging.HookDone(HookName);
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
