#if USE_MINHOOK
using System;
using System.Runtime.InteropServices;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged.Hooks.Native;

/// <summary>
/// This hook hooks GetAddrInfoW and replaces ppy.sh resolves with titanic.sh
/// </summary>
public static class AddrInfoHook
{
    public static void Initialize()
    {
        GetAddrInfoWDelegate hookDelegate = HookedGetAddrInfoW;
        IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(hookDelegate);
        
        var status = MH.CreateHookApiEx("ws2_32", "GetAddrInfoW", hookPtr, out originalGetAddrInfoW, out ppTarget); // Create hook
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to create GetAddrInfoW hook {status}");
            return;
        }
        
        // Get function from original WSAConnect to call it
        originalGetAddrInfoWFunc = (GetAddrInfoWDelegate)Marshal.GetDelegateForFunctionPointer(originalGetAddrInfoW, typeof(GetAddrInfoWDelegate));
        
        // Do not GC our hooking thingies
        GC.KeepAlive(hookDelegate);
        GC.KeepAlive(originalGetAddrInfoWFunc);
        
        status = MH.EnableHook(ppTarget);
        Console.WriteLine($"Enabling AddrInfoHook status: {status}");
    }
    
    #region Hook
    
    private static int HookedGetAddrInfoW(string pNodeName, string pServiceName, IntPtr pHints, IntPtr pResult)
    {
        Console.WriteLine($"GetAddrInfoW hook triggered {pNodeName}");
        if (pNodeName.Contains("ppy.sh"))
        {
            Console.WriteLine($"Replacing ppy.sh resolve to {EntryPoint.Config.ServerName}");
            pNodeName = pNodeName.Replace("ppy.sh", EntryPoint.Config.ServerName);
        }
        return originalGetAddrInfoWFunc(pNodeName, pServiceName, pHints, pResult);
    }
    
    #endregion
    
    #region Delegates
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetAddrInfoWDelegate([MarshalAs(UnmanagedType.LPWStr)] string pNodeName, [MarshalAs(UnmanagedType.LPWStr)] string pServiceName, IntPtr pHints, IntPtr ppResult);

    private static IntPtr ppTarget;
    private static IntPtr originalGetAddrInfoW = IntPtr.Zero;
    private static GetAddrInfoWDelegate originalGetAddrInfoWFunc;
    
    #endregion
}
#endif
