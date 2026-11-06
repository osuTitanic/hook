// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TitanicHook.Core;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Helpers;
using TitanicHook.Core.Hooks.Loading;
using TitanicHook.Core.OsuInterop;

namespace TitanicHook.Loader;

public static class OsuLoader
{
    public static Configuration Config = null!;
    public static bool AutoUpdated = false;

    /// <summary>
    /// Loads osu!.exe into memory, applies hooks, and starts osu!'s main function
    /// </summary>
    /// <param name="osuPath">Path to the osu! executable</param>
    public static void Start(string osuPath)
    {
        if (string.IsNullOrEmpty(osuPath) || !File.Exists(osuPath))
        {
            Logging.LogAndShowError("Couldn't locate the osu! executable!");
            return;
        }

        try
        {
            osuPath = Path.GetFullPath(osuPath);
            Directory.SetCurrentDirectory(Path.GetDirectoryName(osuPath)!);
        }
        catch (Exception e)
        {
            Logging.LogAndShowError($"Failed to open the osu! directory: {e}");
            return;
        }

        if (Config == null)
        {
            Config = new Configuration(Constants.DefaultConfigName);
        }

#if !DEBUG
        // Initialize console if not in debug
        if (Config.EnableConsole)
            WinApi.InitializeConsole();
#endif

        PreLaunchChecks.CheckAll();

        Logging.UseConsoleLogging = Config.EnableConsole;
        Logging.UseFileLogging = Config.LogToFile;
        
        // Load osu!
        Assembly loaded;
        try
        {
            OsuPath = osuPath;
            
            // Remove Mark of the Web which makes it impossible to load assemblies with Assembly.LoadFrom.
            // Have to use Win32 call directly, as .NET's File.Delete does not support alternative data streams.
            WinApi.DeleteFileW(OsuPath + ":Zone.Identifier");

            loaded = Assembly.LoadFrom(OsuPath);
        }
        catch (Exception e)
        {
            Logging.LogAndShowError($"Failed to load osu!.exe\n {e}");
            return;
        }

#if NET40
        if (loaded.ImageRuntimeVersion == "v2.0.50727")
        {
            // CLR abuse to set v2 activation policy at runtime
            // https://reedcopsey.com/2011/09/15/setting-uselegacyv2runtimeactivationpolicy-at-runtime/
            ICLRRuntimeInfo clrRuntimeInfo =
                (ICLRRuntimeInfo)RuntimeEnvironment.GetRuntimeInterfaceAsObject(
                    Guid.Empty,
                    typeof(ICLRRuntimeInfo).GUID);

            clrRuntimeInfo.BindAsLegacyV2Runtime();
        } else {
            // Set the process to be DPI aware
            // This fixes the resolution for some .NET 4.0 builds released in 2015
            SetProcessDPIAware();
        }
#else
        if (loaded.ImageRuntimeVersion != Assembly.GetExecutingAssembly().ImageRuntimeVersion)
        {
            ShowFrameworkMismatch(loaded.ImageRuntimeVersion, Assembly.GetExecutingAssembly().ImageRuntimeVersion);
            return;
        }
#endif

        // Get entry point
        MethodInfo entry = loaded.EntryPoint;
        if (entry == null)
        {
            Logging.LogAndShowError("Entry point not found.");
            return;
        }

        List<string> FakeArgs = new();
        FakeArgs.Add(OsuPath);
        if (OsuVersion.GetVersionNumber() >= 20140811)
            FakeArgs.Add("-go"); // -go arg will bypass the updater in older builds. Always adding it won't work, osu! will show unknown file message

        // Load hooks specific to the loader
        Logging.Info("Loading early hooks");
        PatchManager.Apply(new EntryPointHook(loaded));
        PatchManager.Apply(new ExecutablePathHook(OsuPath));
        // TODO: find out if it's necessary to force the hook to point at loader's icon
        //ExtractIconHook.Initialize(AppDomain.CurrentDomain.FriendlyName);
        PatchManager.Apply(new GetArgsHook(FakeArgs.ToArray()));

        // Hook osu!.exe's entrypoint to execute other hooks there
        // This is required because osu!common has to be loaded by osu! for hooking
        // If we would've loaded osu!common manually it wouldn't work
        Logging.Info("Hooking osu!'s main function");
        PatchManager.Apply(new OsuStartHook(entry));

        // Start the exe's entry point
        Logging.Info("Starting osu!");
        entry.Invoke(null, new object[] { });
    }

    private static void ShowFrameworkMismatch(string osuVer, string hookVer)
    {
        Logging.LogAndShowError($".NET Framework runtime version mismatch!\n" +
                                $"This version of osu! requires .NET Framework {GetShortFrameworkVer(osuVer)}, this Titanic! launcher uses .NET Framework {GetShortFrameworkVer(hookVer)}\n" +
                                $"Get the correct launcher version for this version of osu!.");
    }

    /// <summary>
    /// Converts long framework version (e.g. v2.0.50727, v4.0.30319) to short (e.g. 2.0, 4.0)
    /// </summary>
    /// <param name="framework">Long framework version</param>
    /// <returns>Short framework version</returns>
    private static string GetShortFrameworkVer(string framework)
    {
        return framework.Substring(1, 4);
    }

    internal static string OsuPath = "";

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("BD39D1D2-BA2F-486A-89B0-B4B0CB466891")]
    private interface ICLRRuntimeInfo
    {
        void xGetVersionString();
        void xGetRuntimeDirectory();
        void xIsLoaded();
        void xIsLoadable();
        void xLoadErrorString();
        void xLoadLibrary();
        void xGetProcAddress();
        void xGetInterface();
        void xSetDefaultStartupFlags();
        void xGetDefaultStartupFlags();

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void BindAsLegacyV2Runtime();
    }

    [DllImport("user32.dll")]
    public static extern bool SetProcessDPIAware();
}
