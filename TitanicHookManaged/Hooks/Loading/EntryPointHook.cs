// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Loading;

/// <summary>
/// Hook for spoofing GetEntryAssembly.
/// Only to be used in HookLoader
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
