// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Reflection;
using Harmony;
using TitanicHookManaged;
using TitanicHookManaged.Helpers;

namespace HookLoader;

/// <summary>
/// Hook to load our own hooks after osu!.exe finishes loading but before main is called
/// </summary>
public static class OsuStartHook
{
    private const string HookName = "sh.Titanic.Hook.OsuStartHook";
    
    public static void Initialize(MethodInfo method)
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);
        var prefix = AccessTools.Method(typeof(OsuStartHook), nameof(OsuStartPrefix));
        try
        {
            Logging.HookStep(HookName, "Patching");
            harmony.Patch(method, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
    }
    
    #region Hook
    
    private static void OsuStartPrefix()
    {
        // Load TitanicHook
        Logging.HookTrigger(HookName);
        EntryPoint.InitializeHooks(Program.Config, Program.OsuPath, Program.AutoUpdated);
    }
    
    #endregion
}
