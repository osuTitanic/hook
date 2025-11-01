// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Reflection;
using System.Windows.Forms;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public class WinformSetTitleHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.WinformSetTitle";

    public WinformSetTitleHook() : base(HookName)
    {
        TargetMethods = [typeof(Form).GetMethod("set_Text")];
        Prefixes = [AccessTools.Method(typeof(WinformSetTitleHook), nameof(SetTitlePrefix))];
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
