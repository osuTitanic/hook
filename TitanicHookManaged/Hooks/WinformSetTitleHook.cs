// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Reflection;
using System.Windows.Forms;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public static class WinformSetTitleHook
{
    public const string HookName = "sh.Titanic.Hook.WinformSetTitle";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);

        MethodInfo? setTitle = typeof(Form)
            .GetMethod("set_Text");
        if (setTitle == null)
        {
            Logging.HookError(HookName, "Unable to find set_Text method");
            return;
        }
        
        var prefix = AccessTools.Method(typeof(WinformSetTitleHook), nameof(SetTitlePrefix));

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(setTitle, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
            return;
        }
        
        Logging.HookDone(HookName);
    }

    #region Hook
    
    private static void SetTitlePrefix(ref string value)
    {
        Logging.HookTrigger(HookName);
        if (value.StartsWith("osu!"))
            value = $"(Titanic) {value}";
    }

    #endregion
}