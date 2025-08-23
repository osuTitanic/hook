// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public static class StartProcessHook
{
    public const string HookName = "sh.Titanic.Hook.StartProcess";
    
    /// <summary>
    /// Regex for the copyright link
    /// </summary>
    private static readonly Regex CopyrightRegex = new Regex(@"^https?:\/\/osu\.ppy\.sh\/?$");
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);
        
        MethodInfo? targetMethod = typeof(Process)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "Start" &
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType.FullName == "System.Diagnostics.ProcessStartInfo");

        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Couldn't find Process.Start(ProcessStartInfo)");
            return;
        }

        var prefix = typeof(StartProcessHook).GetMethod("ProcessStartPrefix", Constants.HookBindingFlags);

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName,e.ToString());
        }
        
        Logging.HookDone(HookName);
    }

    #region Hook

    private static void ProcessStartPrefix(ref ProcessStartInfo __0)
    {
        Logging.HookTrigger(HookName);
        
        // Do not replace the link for copyright button
        if (CopyrightRegex.IsMatch(__0.FileName)) return;
        
        if (__0.FileName.Contains("ppy.sh")) // TODO: Make regex check for URLs
        {
            __0.FileName = __0.FileName.Replace("ppy.sh", EntryPoint.Config.ServerName);
        }
    }

    #endregion
}
