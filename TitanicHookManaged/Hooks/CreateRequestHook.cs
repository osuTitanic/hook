// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

#if NET40
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Hook for create request method in WebRequest class
/// </summary>
public static class CreateRequestHook
{
    public const string HookName = "sh.Titanic.Hook.CreateRequest";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        var harmony = HarmonyInstance.Create(HookName);
        
        // Look for the Create(string) overload
        MethodInfo? targetMethod = typeof(WebRequest).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "Create" &&
                                 m.GetParameters().Length == 1 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.String");
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Target method not found", false);
            return;
        }
        
        Logging.HookStep(HookName,$"Resolved create request method: {targetMethod.DeclaringType?.FullName}.{targetMethod.Name}");
        
        var prefix = typeof(CreateRequestHook).GetMethod("CreateRequestPrefix", Constants.HookBindingFlags);

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
    
    private static void CreateRequestPrefix(ref string __0)
    {
        Logging.HookTrigger(HookName);
        if (__0.Contains("ppy.sh"))
        {
            Logging.HookOutput(HookName, $"Replacing ppy.sh domain with {EntryPoint.Config.ServerName} in WebRequest.Create(string)");
            var regex = new Regex(Regex.Escape("ppy.sh"));
            __0 = regex.Replace(__0, EntryPoint.Config.ServerName, 1); // only the first occurence
        }
    }
    
    #endregion
}
#endif // NET40
