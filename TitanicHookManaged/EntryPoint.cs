using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Hooks.Managed;
using TitanicHookShared;
#if USE_MINHOOK
using TitanicHookManaged.Hooks.Native;
using TitanicHookManaged.MinHook;
#endif

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
        
#if USE_MINHOOK
        var status = MH.Initialize();
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to initialize MinHook: {status}");
            //MessageBox.Show($"Failed to initialize MinHook: {status}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            // Native hooks
            if (Config.HookTcpConnections)
            {
                Console.WriteLine("Hooking WSAConnect");
                WSAConnectRedirect.Initialize();
            }
            Console.WriteLine("Hooking GetAddrInfo");
            AddrInfoHook.Initialize();
            Console.WriteLine("Hooking ShellExecuteExW");
            ShellExecuteHook.Initialize();
        }
#else

        // Managed hooks
        if (Config.HookTcpConnections)
        {
            TcpClientHook.Initialize();
        }
        DnsHostByNameHook.Initialize();
        StartProcessHook.Initialize();

#endif
        
        if (Config.HookNetLib)
        {
            AddHeaderFieldHook.Initialize();
            NetLibEncodingHook.Initialize();
        }
        
#if NET40
        if (Config.HookModernHostMethod)
        {
            HostHeaderHook.Initialize();
        }

        if (Config.HookCheckCertificate)
        {
            CheckCertificateHook.Initialize();
        }
#endif
        
        Logging.Info("All hooked");
    }

    public static Configuration? Config = null;
}
