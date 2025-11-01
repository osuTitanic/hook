// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public class DnsHostByNameHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.DnsHostByName";

    public DnsHostByNameHook() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Prefixes = [AccessTools.Method(typeof(DnsHostByNameHook), nameof(InternalGetHostByNamePrefix))];
    }

    private static MethodInfo GetTargetMethod()
    {
        return typeof(Dns)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == "InternalGetHostByName" && m.GetParameters().Length == 2);
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
