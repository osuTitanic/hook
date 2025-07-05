using System;
using System.IO;
using System.Windows.Forms;
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
        
        var status = MinHook.MinHook.MH_Initialize();
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine("Failed to initialize MH");
        }
        
        WSAConnectRedirect.Initialize();
        
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