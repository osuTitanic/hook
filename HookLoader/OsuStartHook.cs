// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Reflection;
using Harmony;
using TitanicHookManaged;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace HookLoader;

/// <summary>
/// Hook to load our own hooks after osu!.exe finishes loading but before main is called
/// </summary>
public class OsuStartHook : TitanicPatch
{
    private const string HookName = "sh.Titanic.Hook.OsuStartHook";

    public OsuStartHook(MethodInfo method) : base(HookName)
    {
        TargetMethods = [method];
        Prefixes = [AccessTools.Method(typeof(OsuStartHook), nameof(OsuStartPrefix))];
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
