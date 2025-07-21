using System;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Hooks.Managed;
using TitanicHookManaged.Hooks.Native;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged;

public static class EntryPoint
{
    /// <summary>
    /// Initializes the console after injecting
    /// </summary>
    private static void InitializeConsole()
    {
        WinApi.AllocConsole();
        
        var stdout = WinApi.GetStdHandle(WinApi.STD_OUTPUT_HANDLE);
        var stderr = WinApi.GetStdHandle(WinApi.STD_ERROR_HANDLE);
        var outFs = new FileStream(stdout, FileAccess.Write);
        var outWriter = new StreamWriter(outFs)
        {
            AutoFlush = true
        };
        var errFs = new FileStream(stderr, FileAccess.Write);
        var errWriter = new StreamWriter(errFs)
        {
            AutoFlush = true
        };
        Console.SetOut(outWriter);
        Console.SetError(errWriter);
    }

    /// <summary>
    /// Target for injecting with ManagedInjector
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static int InjectedTarget(string args)
    {
        InitializeConsole();
        InitializeHooks();
        return 0;
    }
    
    /// <summary>
    /// Start hooks
    /// </summary>
    public static void InitializeHooks()
    {
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Console.WriteLine("Unhandled exception in injected module " + e.ExceptionObject.ToString());
        };
        
        var status = MH.Initialize();
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to initialize MinHook: {status}");
            //MessageBox.Show($"Failed to initialize MinHook: {status}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            // Native hooks
            Console.WriteLine("Hooking WSAConnect");
            WSAConnectRedirect.Initialize();
            Console.WriteLine("Hooking GetAddrInfo");
            AddrInfoHook.Initialize();
            Console.WriteLine("Hooking ShellExecuteExW");
            ShellExecuteHook.Initialize();
        }
        
        // Managed hooks
#if NET20
        Console.WriteLine("Hooking AddHeaderField");
        AddHeaderFieldHook.Initialize();
        Console.WriteLine("Hooking StringStream ctor");
        NetLibEncodingHook.Initialize();
#endif
#if NET40
        Console.WriteLine("Hooking CreateWebRequest");
        HostHeaderHook.Initialize();
        Console.WriteLine("Hooking checkCertificate");
        CheckCertificateHook.Initialize();
#endif
        Console.WriteLine("All hooked");
    }
}
