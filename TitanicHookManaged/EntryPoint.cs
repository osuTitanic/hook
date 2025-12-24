// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Hooks.Connection;
using TitanicHookManaged.Hooks.Fixes;
using TitanicHookManaged.Hooks.Loading;
using TitanicHookManaged.Hooks.Misc;
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

        PatchManager.Apply(new WinformSetTitleHook());
        PatchManager.Apply(new NowPlayingCommandHook());
        if (Config.RemoveScoreFetchingDelay) PatchManager.Apply(new RemoveScoreDelayHook());
        
        if (Config.HookTcpConnections) PatchManager.Apply(new TcpClientHook());
        PatchManager.Apply(new DnsHostByNameHook());
        PatchManager.Apply(new StartProcessHook());
        PatchManager.Apply(new BeatmapSubmissionLinksPatch());
        PatchManager.Apply(new DisableRegistryPatch());
        
        if (Config.HookNetLibHeaders) PatchManager.Apply(new AddHeaderFieldHook());
        if (Config.HookNetLibEncoding) PatchManager.Apply(new NetLibEncodingHook());
        
        if (Config.RemovePeppyDmCheck) PatchManager.Apply(new AskPeppyFix());
        
#if NET40
        PatchManager.Apply(new HostHeaderHook());
        PatchManager.Apply(new CreateRequestHook());
        if (Config.HookCheckCertificate) PatchManager.Apply(new CheckCertificateHook());
#endif
        
        PluginLoader.LoadPlugins();
        
        Logging.Info("All hooked");
        Config.FirstRun = false;
        Config.SaveConfiguration(Config.Filename);
        string notifMessage = $"Welcome to Titanic! (v{Constants.PatchVersion})";
        if (autoUpdated)
            notifMessage += "\nUpdated successfully!";
        Notifications.I.ShowMessage(notifMessage);
    }

    public static Configuration? Config = null;
}
