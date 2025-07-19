using System;
using System.Runtime.InteropServices;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// This hook hooks GetAddrInfoW and replaces ppy.sh resolves with titanic.sh
/// </summary>
public static class AddrInfoHook
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetAddrInfoWDelegate([MarshalAs(UnmanagedType.LPWStr)] string pNodeName, [MarshalAs(UnmanagedType.LPWStr)] string pServiceName, IntPtr pHints, IntPtr ppResult);
    
    private static IntPtr originalGetAddrInfoW = IntPtr.Zero;
    private static GetAddrInfoWDelegate originalGetAddrInfoWFunc;

    private static int HookedGetAddrInfoW(string pNodeName, string pServiceName, IntPtr pHints, IntPtr pResult)
    {
        Console.WriteLine($"GetAddrInfoW hook triggered {pNodeName}");
        if (pNodeName.Contains("ppy.sh"))
        {
            Console.WriteLine("Replacing ppy.sh resolve to titanic.sh");
            pNodeName = pNodeName.Replace("ppy.sh", "titanic.sh");
        }
        return originalGetAddrInfoWFunc(pNodeName, pServiceName, pHints, pResult);
    }
    
    public static void Initialize()
    {
        GetAddrInfoWDelegate hookDelegate = HookedGetAddrInfoW;
        IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(hookDelegate);
        
        IntPtr hWS2 = WinApi.LoadLibrary("ws2_32.dll"); // Load target lib
        IntPtr GetAddrInfoWHandle = WinApi.GetProcAddress(hWS2, "GetAddrInfoW"); // Get the address of function we are hooking
        
        var status = MH.CreateHook(GetAddrInfoWHandle, hookPtr, out originalGetAddrInfoW); // Create hook
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to create GetAddrInfoW hook {status}");
        }
        
        // Get function from original WSAConnect to call it
        originalGetAddrInfoWFunc = (GetAddrInfoWDelegate)Marshal.GetDelegateForFunctionPointer(originalGetAddrInfoW, typeof(GetAddrInfoWDelegate));
        
        // Do not GC our hooking thingies
        GC.KeepAlive(hookDelegate);
        GC.KeepAlive(originalGetAddrInfoWFunc);
    }
}
