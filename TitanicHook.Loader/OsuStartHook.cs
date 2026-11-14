// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Reflection;
using Harmony;
using TitanicHook.Core;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Helpers;
using TitanicHook.Loader;

namespace TitanicHook.Loader;

/// <summary>
/// Hook to load our own hooks after osu!.exe finishes loading but before main is called
/// </summary>
public class OsuStartHook : TitanicPatch
{
    private const string LoaderHookName = "sh.Titanic.Hook.OsuStartHook";

    public OsuStartHook(MethodInfo method) : base(LoaderHookName)
    {
        TargetMethods = [method];
        Prefixes = [AccessTools.Method(typeof(OsuStartHook), nameof(OsuStartPrefix))];
    }
    
    #region Hook
    
    private static void OsuStartPrefix()
    {
        // Load TitanicHook
        Logging.HookTrigger(LoaderHookName);
        EntryPoint.InitializeHooks(OsuLoader.Config, OsuLoader.OsuPath, OsuLoader.AutoUpdated);
    }
    
    #endregion
}
