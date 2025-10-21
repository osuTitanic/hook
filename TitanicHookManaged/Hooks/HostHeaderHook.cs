// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

#if NET40
using System;
using System.Net;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Hook for Host header in clients using pWebRequest.
/// This works by making a prefix in setter of HttpWebRequest.Host and changing the passed in domain.
/// </summary>
public static class HostHeaderHook
{
    public const string HookName = "sh.Titanic.Hook.HostHeader";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        var harmony = HarmonyInstance.Create(HookName);
        
        MethodInfo? targetMethod = typeof(HttpWebRequest).GetMethod("set_Host", BindingFlags.Instance | BindingFlags.Public);
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Target method not found", false);
            return;
        }
        
        Logging.HookStep(HookName,$"Resolved set_Host: {targetMethod.DeclaringType?.FullName}.{targetMethod.Name}");
        
        var prefix = AccessTools.Method(typeof(HostHeaderHook), nameof(SetHostPrefix));

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
    
    private static void SetHostPrefix(ref string __0)
    {
        Logging.HookTrigger(HookName);
        if (__0.Contains("ppy.sh"))
        {
            Logging.HookOutput(HookName, $"Replacing ppy.sh domain with {EntryPoint.Config.ServerName} in set_Host");
            __0 = __0.Replace("ppy.sh", EntryPoint.Config.ServerName);
        }
    }
    
    #endregion
}
#endif // NET40
