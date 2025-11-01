// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Managed hook that will replace the value of the Host HTTP header with the desired private server domain
/// </summary>
public class AddHeaderFieldHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.AddHeaderField";

    public AddHeaderFieldHook() : base(HookName)
    {
        TargetMethods = [GetTargetMethod(AssemblyUtils.CommonOrOsuTypes)];
        Prefixes = [AccessTools.Method(typeof(AddHeaderFieldHook), nameof(AddHeaderFieldPrefix))];
    }
    
    #region Hook
    
    /// <summary>
    /// Hooked AddHeaderField
    /// </summary>
    /// <param name="__1">Name of the header</param>
    /// <param name="__2">Value of the header. It's ref here so that we can get it by reference and modify it</param>
    private static void AddHeaderFieldPrefix(string __1, ref string __2)
    {
        Logging.HookTrigger(HookName);
        
        if (__1 == "Host" && __2.Contains("ppy.sh"))
            __2 = __2.Replace("ppy.sh", EntryPoint.Config.ServerName);
        else if (__1 == "Host" && __2 == "peppy.chigau.com")
            __2 = __2.Replace("peppy.chigau.com", $"chigau.{EntryPoint.Config.ServerName}");
    }
    
    #endregion

    
    #region Find method
    
    /// <summary>
    /// Find target method to hook
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private static MethodInfo? GetTargetMethod(Type[] types)
    {
        MethodInfo? targetMethod = types
            .SelectMany(m => m.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .FirstOrDefault(m => m.GetParameters().Length == 3 &&
                                 m.GetParameters()[0].ParameterType.FullName ==
                                 "System.Collections.Specialized.StringCollection" &&
                                 m.GetParameters()[1].ParameterType.FullName == "System.String" &&
                                 m.GetParameters()[2].ParameterType.FullName == "System.String" &&
                                 m.ReturnType.FullName == "System.Void");
        
        return targetMethod;
    }
    
    #endregion
}
