using System;
using System.IO;
using System.Net;
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
        
        Console.WriteLine("Resolving server.titanic.sh IP");
        IPAddress ip = Dns.GetHostAddresses("server.titanic.sh")[0];
        WSAConnectRedirect.TITANIC_IP_BE = ip.GetAddressBytes();
        Console.WriteLine("Titanic IP: " + ip);
        
        var status = MinHook.MinHook.MH_Initialize();
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine("Failed to initialize MH");
        }
        
        WSAConnectRedirect.Initialize();
        AddrInfoHook.Initialize();
        AddHeaderFieldHook.Initialize();
        
        // Enable all hooks
        status = MinHook.MinHook.MH_EnableHook(IntPtr.Zero);
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine("Failed to enable hooks");
        }
        
        Console.WriteLine("All done");
        return 0;
    }
}