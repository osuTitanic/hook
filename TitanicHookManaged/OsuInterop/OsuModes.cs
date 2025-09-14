// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.OsuInterop;

public static class OsuModes
{
    /// <summary>
    /// Reference to OsuModes enum.
    /// It's accessible in unobfuscated form so we can reflect it by name
    /// </summary>
    static Type? _osuModeEnum = AssemblyUtils.OsuTypes
        .FirstOrDefault(t => t.IsEnum && t.IsPublic && t.Name == "OsuModes");

    private static MethodInfo? _changeModeMethod = AssemblyUtils.OsuTypes
        .Where(t => t is { IsClass: true, IsNested: false })
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
        .FirstOrDefault(m => m.ReturnType.FullName == "System.Void" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType.Name == "OsuModes" && m.GetParameters()[1].ParameterType.FullName == "System.Boolean");
    
    private static Dictionary<string, int> osuModesEnumDict = new();
    
    static OsuModes()
    {
        foreach (var field in _osuModeEnum?.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            string name = field.Name;
            object rawValue = field.GetValue(null);
            object underlyingValue = Convert.ChangeType(rawValue, Enum.GetUnderlyingType(_osuModeEnum));
            int intValue = Convert.ToInt32(underlyingValue);
            osuModesEnumDict.Add(name, intValue);
        }
    }

    public static int? TryGetMode(string key)
    {
        if (osuModesEnumDict.TryGetValue(key, out int mode))
        {
            return mode;
        }
        return null;
    }

    /// <summary>
    /// Tries to change the gamemode
    /// </summary>
    /// <param name="modeName">Mode name</param>
    public static void ChangeMode(string modeName)
    {
        int? mode = TryGetMode(modeName);
        if (mode == null)
        {
            Logging.Info($"Couldn't find ID for mode {modeName}");
            return;
        }
        
        Logging.Info($"Switching mode to {modeName} (ID: {mode})");
        _changeModeMethod?.Invoke(null, [mode, true]);
    }
}
