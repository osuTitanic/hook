// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Hook for spoofing Application.ExecutablePath getter.
/// Only to be used by HookLoader
/// </summary>
public class ExecutablePathHook : TitanicPatch
{
    private static string? _spoofedExePath;
    public const string HookName = "sh.Titanic.Hook.ExecutablePath";

    public ExecutablePathHook(string? spoofedExePath) : base(HookName)
    {
        _spoofedExePath = spoofedExePath;
        TargetMethods =
        [
            typeof(System.Windows.Forms.Application).GetMethod("get_ExecutablePath",
                BindingFlags.Static | BindingFlags.Public)
        ];
        Prefixes = [AccessTools.Method(typeof(ExecutablePathHook), nameof(GetExecutablePathPrefix))];
    }
    
    #region Hook
    
    private static bool GetExecutablePathPrefix(ref string? __result)
    {
        Logging.HookTrigger(HookName);
        __result = _spoofedExePath;
        return false;
    }
    
    #endregion
}
