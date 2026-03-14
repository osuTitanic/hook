// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Reflection;
using Harmony;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Helpers;

namespace TitanicHook.Core.Hooks.Loading;

/// <summary>
/// Hook for spoofing GetEntryAssembly.
/// Only to be used in TitanicHook.Loader
/// </summary>
public class EntryPointHook : TitanicPatch
{
    private static Assembly? _spoofedEntryPointAssembly;
    public const string HookName = "sh.Titanic.Hook.GetEntryAssembly";

    public EntryPointHook(Assembly? spoofedAssembly) : base(HookName)
    {
        _spoofedEntryPointAssembly = spoofedAssembly;
        TargetMethods = [typeof(Assembly).GetMethod("GetEntryAssembly", BindingFlags.Static | BindingFlags.Public)];
        Prefixes = [AccessTools.Method(typeof(EntryPointHook), nameof(GetEntryAssemblyPrefix))];
    }
    
    #region Hook
    
    private static bool GetEntryAssemblyPrefix(ref Assembly? __result)
    {
        Logging.HookTrigger(HookName);
        __result = _spoofedEntryPointAssembly;
        return false;
    }
    
    #endregion
}
