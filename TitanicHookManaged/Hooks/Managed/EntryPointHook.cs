﻿using System;
using System.Reflection;
using HarmonyLib;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for spoofing GetEntryAssembly.
/// Only to be used in HookLoader
/// </summary>
public static class EntryPointHook
{
    private static Assembly? _spoofedEntryPointAssembly;
    public const string HookName = "sh.Titanic.Hook.GetEntryAssembly";
    
    public static void Initialize(Assembly? spoofedAssembly)
    {
        Logging.HookTrigger(HookName);
        
        var harmony = new Harmony(HookName);
        _spoofedEntryPointAssembly = spoofedAssembly;

        MethodInfo? targetMethod = typeof(Assembly).GetMethod("GetEntryAssembly", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Could not find entry assembly target method");
            return;
        }
        
        var prefix = typeof(EntryPointHook).GetMethod("GetEntryAssemblyPrefix", Constants.HookBindingFlags);

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
    
    private static bool GetEntryAssemblyPrefix(ref Assembly? __result)
    {
        Logging.HookTrigger(HookName);
        __result = _spoofedEntryPointAssembly;
        return false;
    }
    
    #endregion
}
