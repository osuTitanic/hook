using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged;

public static class EntryPoint
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WSAConnectDelegate(IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData,
        IntPtr lpCaleeData, IntPtr lpSQOS, IntPtr lpGQOS);
    
    private static IntPtr originalWSAConnect = IntPtr.Zero;
    private static WSAConnectDelegate originalWSAConnectFunc;

    private static int HookedWSAConnect(
        IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData, IntPtr lpCaleeData, IntPtr lpSQOS, IntPtr lpGQOS)
    {
        MessageBox.Show("WSAConnect hook triggered", "Hook trigger");
        return originalWSAConnectFunc(s, name, namelen, lpCallerData, lpCaleeData, lpSQOS, lpGQOS);
    }
    
    public static int Start(string args)
    {
        MessageBox.Show("Start hook triggered", "Hook trigger");
        WinApi.AllocConsole();
        
        Console.WriteLine("Hello World!");
        
        var status = MinHook.MinHook.MH_Initialize();
        if (status != MhStatus.MH_OK)
        {
            MessageBox.Show("Failed to initialize MH");
        }
        
        WSAConnectDelegate hookDelegate = HookedWSAConnect;
        IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(hookDelegate);

        IntPtr hWS2 = WinApi.LoadLibrary("ws2_32.dll");
        IntPtr WSAConnectHandle = WinApi.GetProcAddress(hWS2, "WSAConnect");
        status = MinHook.MinHook.MH_CreateHook(WSAConnectHandle, hookPtr, out originalWSAConnect);
        
        if (status != MhStatus.MH_OK)
        {
            MessageBox.Show($"Failed to create WSAConnect hook {status}");
        }
        
        originalWSAConnectFunc = (WSAConnectDelegate)Marshal.GetDelegateForFunctionPointer(originalWSAConnect, typeof(WSAConnectDelegate));
        
        status = MinHook.MinHook.MH_EnableHook(IntPtr.Zero);
        if (status != MhStatus.MH_OK)
        {
            MessageBox.Show("Failed to enable hook");
        }
        
        GC.KeepAlive(hookDelegate);
        GC.KeepAlive(originalWSAConnectFunc);
        
        MessageBox.Show("All done", "test");
        return 0;
    }
}