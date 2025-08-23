// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Linq;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Managed hook that will replace the value of the Host HTTP header with the desired private server domain
/// </summary>
public static class AddHeaderFieldHook
{
    public const string HookName = "sh.Titanic.Hook.AddHeaderField";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);

        MethodInfo? targetMethod = GetTargetMethod(AssemblyUtils.CommonOrOsuTypes);
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Target method not found", !EntryPoint.Config.FirstRun);
            if (EntryPoint.Config.FirstRun)
                EntryPoint.Config.HookNetLibHeaders = false;
            return;
        }
        Logging.HookStep(HookName, $"Resolved AddHeaderField: {targetMethod.Name}");
        
        var prefix = typeof(AddHeaderFieldHook).GetMethod("AddHeaderFieldPrefix", Constants.HookBindingFlags);

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
