// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Drawing;
using System.Linq;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Hook for ExtractAssociatedIcon so that osu! will have correct icon.
/// Only to be used in HookLoader
/// </summary>
public class ExtractIconHook : TitanicPatch
{
    private static string? _hookLoaderName;
    public const string HookName = "sh.Titanic.Hook.ExtractIcon";

    public ExtractIconHook(string? hookLoaderName) : base(HookName)
    {
        if (hookLoaderName == null)
            return;
        
        _hookLoaderName = hookLoaderName;

        TargetMethods = [GetTargetMethod()];
        Prefixes = [AccessTools.Method(typeof(ExtractIconHook), nameof(ExtractAssociatedIconPrefix))];
    }

    private static MethodInfo? GetTargetMethod()
    {
        // We want specifically the overload that takes System.String
        return typeof(Icon)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "ExtractAssociatedIcon" &&
                                 m.GetParameters().Length == 1 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.String"
            );
    }
    
    #region Hook
    
    private static void ExtractAssociatedIconPrefix(ref string __0)
    {
        Logging.HookTrigger(HookName);
        __0 = __0.Replace(_hookLoaderName, "osu!.exe"); // Change the target icon path
    }
    
    #endregion
}
