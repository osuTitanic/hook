// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for spoofing Application.ExecutablePath getter.
/// Only to be used by HookLoader
/// </summary>
public static class ExecutablePathHook
{
    private static string? _spoofedExePath;
    public const string HookName = "sh.Titanic.Hook.ExecutablePath";
    
    public static void Initialize(string? spoofedExePath)
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);
        _spoofedExePath = spoofedExePath;

        MethodInfo? targetMethod = typeof(System.Windows.Forms.Application).GetMethod("get_ExecutablePath", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Could not find get_ExecutablePath method");
            return;
        }
        
        var prefix = typeof(ExecutablePathHook).GetMethod("GetExecutablePathPrefix", Constants.HookBindingFlags);

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
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
