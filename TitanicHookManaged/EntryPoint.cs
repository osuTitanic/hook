using System;
using System.IO;
using System.Net;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Hooks;
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
        var fs = new FileStream(stdout, FileAccess.Write);
        var writer = new StreamWriter(fs)
        {
            AutoFlush = true
        };
        Console.SetOut(writer);
        Console.SetError(writer);
    }
    
    public static int Start(string args)
    {
        InitializeConsole();
        Console.WriteLine("Hello from hook world");

        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            Console.WriteLine("Unhandled exception in injected module " + e.ExceptionObject.ToString());
        };
        
        Console.WriteLine("Resolving server.titanic.sh IP");
        IPAddress ip = Dns.GetHostAddresses("server.titanic.sh")[0];
        WSAConnectRedirect.TITANIC_IP_BE = ip.GetAddressBytes();
        Console.WriteLine("Titanic IP: " + ip);
        
        var status = MH.Initialize();
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to initialize MH: {status}");
        }
        
        Console.WriteLine("Hooking WSAConnect");
        WSAConnectRedirect.Initialize();
        Console.WriteLine("Hooking GetAddrInfo");
        AddrInfoHook.Initialize();
        // Console.WriteLine("Hooking AddHeaderField");
        // AddHeaderFieldHook.Initialize();
        // Console.WriteLine("Hooking StringStream ctor");
        // NetLibEncodingHook.Initialize();
        Console.WriteLine("Hooking ShellExecuteExW");
        ShellExecuteHook.Initialize();
#if NET40
        Console.WriteLine("Hooking CreateWebRequest");
        HostHeaderHook.Initialize();
        Console.WriteLine("Hooking checkCertificate");
        CheckCertificateHook.Initialize();
#endif
        Console.WriteLine("All hooked");
        
        // Enable all hooks
        status = MH.EnableHook(IntPtr.Zero);
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to enable hooks: {status}");
        }
        
        Console.WriteLine("All done");
        return 0;
    }
}
