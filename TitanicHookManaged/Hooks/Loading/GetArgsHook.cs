// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Loading;

/// <summary>
/// Hook for spoofing GetCommandLineArgs.
/// Only to be used by HookLoader
/// </summary>
public class GetArgsHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.GetArgs";
    private static string[] _spoofedArgs;

    public GetArgsHook(string[] spoofedArgs) : base(HookName)
    {
        _spoofedArgs = spoofedArgs;
        TargetMethods =
            [typeof(Environment).GetMethod("GetCommandLineArgs", BindingFlags.Static | BindingFlags.Public)];
        Prefixes = [AccessTools.Method(typeof(GetArgsHook), nameof(GetArgsPrefix))];
    }

    #region Hook

    private static bool GetArgsPrefix(ref string[] __result)
    {
        Logging.HookTrigger(HookName);
        __result = _spoofedArgs;
        return false;
    }

    #endregion
    
}
