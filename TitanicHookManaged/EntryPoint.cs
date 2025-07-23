using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Hooks.Managed;
using TitanicHookShared;

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
    public static void InitializeHooks(Configuration? config = null)
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
        
        Logging.UseConsoleLogging = Config.EnableConsole;
        Logging.UseFileLogging = Config.LogToFile;
        
        if (Config.FirstRun)
        {
            // Determine is TCP hook required
            int year = AssemblyUtils.DetectOsuYear(AssemblyUtils.OsuAssembly);
            if (year >= 2014)
                Config.HookTcpConnections = false;
        }
        
        if (Config.HookTcpConnections) TcpClientHook.Initialize();
        DnsHostByNameHook.Initialize();
        StartProcessHook.Initialize();
        
        if (Config.HookNetLibHeaders) AddHeaderFieldHook.Initialize();
        if (Config.HookNetLibEncoding) NetLibEncodingHook.Initialize();
        
#if NET40
        
        if (Config.HookModernHostMethod) HostHeaderHook.Initialize();
        if (Config.HookCheckCertificate) CheckCertificateHook.Initialize();
#endif
        
        Logging.Info("All hooked");
        Config.FirstRun = false;
        Config.SaveConfiguration(Config.Filename);
    }

    public static Configuration? Config = null;
}
