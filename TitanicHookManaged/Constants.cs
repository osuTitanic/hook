// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Reflection;

namespace TitanicHookManaged;

public static class Constants
{
    /// <summary>
    /// Binding flags for reflecting managed hooks
    /// </summary>
    public const BindingFlags HookBindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

    public const string DefaultConfigName = "Titanic!.cfg";
    public const string LogFileName = "Titanic!.log";
    public const string UpdateServer = "http://hook.titanic.sh";
    public const string PatchVersion = "0.2.0";
}
