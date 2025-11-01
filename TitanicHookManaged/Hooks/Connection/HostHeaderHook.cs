// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

#if NET40
using System.Net;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Connection;

/// <summary>
/// Hook for Host header in clients using pWebRequest.
/// This works by making a prefix in setter of HttpWebRequest.Host and changing the passed in domain.
/// </summary>
public class HostHeaderHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.HostHeader";

    public HostHeaderHook() : base(HookName)
    {
        TargetMethods = [typeof(HttpWebRequest).GetMethod("set_Host", BindingFlags.Instance | BindingFlags.Public)];
        Prefixes = [AccessTools.Method(typeof(HostHeaderHook), nameof(SetHostPrefix))];
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
