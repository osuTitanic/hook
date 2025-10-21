// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public static class DnsHostByNameHook
{
    public const string HookName = "sh.Titanic.Hook.DnsHostByName";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);
        
        MethodInfo? targetMethod = typeof(Dns)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == "InternalGetHostByName" && m.GetParameters().Length == 2);

        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Failed to find Dns.InternalGetHostByName(string, bool)");
            return;
        }
        
        var prefix = AccessTools.Method(typeof(DnsHostByNameHook), nameof(InternalGetHostByNamePrefix));

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

    private static void InternalGetHostByNamePrefix(ref string __0)
    {
        Logging.HookTrigger(HookName);
        if (__0.Contains("ppy.sh"))
            __0 = __0.Replace("ppy.sh", EntryPoint.Config.ServerName);
        else if (__0 == "peppy.chigau.com")
            __0 = __0.Replace("peppy.chigau.com", $"chigau.{EntryPoint.Config.ServerName}");
    }
    
    #endregion
}
