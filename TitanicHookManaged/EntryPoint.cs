// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Hooks;
using TitanicHookManaged.OsuInterop;

namespace TitanicHookManaged;

public static class EntryPoint
{
    /// <summary>
    /// Target for injecting with ManagedInjector
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static int InjectedTarget(string args)
    {
        WinApi.InitializeConsole();
        InitializeHooks();
        return 0;
    }
    
    /// <summary>
    /// Start hooks
    /// </summary>
    public static void InitializeHooks(Configuration? config = null, string osuPath = "", bool autoUpdated = false)
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Logging.LogAndShowError("Unhandled exception in injected module " + e.ExceptionObject.ToString());
        };

        if (config == null)
            Config ??= new Configuration(Constants.DefaultConfigName);
        else
            Config = config;
        
        if (Config.EnableConsole)
            WinApi.InitializeConsole();

        // Try to get osu! path with reflection if it wasn't passed in as arg to this func
        if (osuPath == "")
            osuPath = AssemblyUtils.OsuAssembly.Location;

        if (osuPath != "")
        {
            string currentClientSha256 = ChecksumUtils.CalculateSha256(osuPath);
            if (Config.ClientSha256 == "")
                Config.ClientSha256 = currentClientSha256;

#if !DEBUG
            if (currentClientSha256 != Config.ClientSha256)
            {
                Logging.LogAndShowError("This configuration file was created for a different version of osu!\n" +
                                        $"Please delete {Constants.DefaultConfigName} and try again.");
                Environment.Exit(1);
            }
#endif
        }
        
        Logging.UseConsoleLogging = Config.EnableConsole;
        Logging.UseFileLogging = Config.LogToFile;
        
        if (Config.FirstRun && osuPath != "")
        {
            // Determine is TCP hook required
            int year = AssemblyUtils.DetectOsuYear(osuPath);
            if (year >= 2014)
                Config.HookTcpConnections = false;
        }
        
        Logging.Info($"osu! version from reflection: {OsuVersion.GetVersion()}");
        
        WinformSetTitleHook.Initialize();
        NowPlayingCommandHook.Initialize();
        if (Config.RemoveScoreFetchingDelay) RemoveScoreDelayHook.Initialize();
        
        if (Config.HookTcpConnections) TcpClientHook.Initialize();
        DnsHostByNameHook.Initialize();
        StartProcessHook.Initialize();
        BeatmapSubmissionLinksPatch.Initialize();
        DisableRegistryPatch.Initialize();
        
        if (Config.HookNetLibHeaders) AddHeaderFieldHook.Initialize();
        if (Config.HookNetLibEncoding) NetLibEncodingHook.Initialize();
        
#if NET40
        
        HostHeaderHook.Initialize();
        CreateRequestHook.Initialize();
        if (Config.HookCheckCertificate) CheckCertificateHook.Initialize();
#endif
        
        BenchmarkSubmitPatch.Initialize();
        
        Logging.Info("All hooked");
        Config.FirstRun = false;
        Config.SaveConfiguration(Config.Filename);
        string notifMessage = $"Welcome to Titanic! (v{Constants.PatchVersion})";
        if (autoUpdated)
            notifMessage += "\nUpdated successfully!";
        Notifications.ShowMessage(notifMessage);
    }

    public static Configuration? Config = null;
}
