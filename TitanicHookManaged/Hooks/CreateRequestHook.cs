// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

#if NET40
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Hook for create request method in WebRequest class
/// </summary>
public class CreateRequestHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.CreateRequest";

    public CreateRequestHook() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Prefixes = [AccessTools.Method(typeof(CreateRequestHook), nameof(CreateRequestPrefix))];
    }

    private static MethodInfo? GetTargetMethod()
    {
        // Look for the Create(string) overload
        return typeof(WebRequest).GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "Create" &&
                                 m.GetParameters().Length == 1 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.String");
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
