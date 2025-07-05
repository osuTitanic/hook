using System;
using System.Windows.Forms;
using TitanicHookManaged.Hooks;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged;

public static class EntryPoint
{
    public static int Start(string args)
    {
        MessageBox.Show("Start hook triggered", "Hook trigger");
        WinApi.AllocConsole();
        
        Console.WriteLine("Hello World!"); // idk why doesn't show
        
        var status = MinHook.MinHook.MH_Initialize();
        if (status != MhStatus.MH_OK)
        {
            MessageBox.Show("Failed to initialize MH");
        }
        
        WSAConnectRedirect.Initialize();
        
        // Enable all hooks
        status = MinHook.MinHook.MH_EnableHook(IntPtr.Zero);
        if (status != MhStatus.MH_OK)
        {
            MessageBox.Show("Failed to enable hook");
        }
        
        MessageBox.Show("All done");
        return 0;
    }
}