using System;
using System.Runtime.InteropServices;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// This hook will redirect all socket openings to Titanic's server IP
/// </summary>
public class WSAConnectRedirect
{
    // Delegate of WSAConnect
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WSAConnectDelegate(IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData,
        IntPtr lpCaleeData, IntPtr lpSQOS, IntPtr lpGQOS);
    
    
    private static IntPtr originalWSAConnect = IntPtr.Zero; // Pointer to original WSAConnect
    private static WSAConnectDelegate originalWSAConnectFunc; // Delegate to original WSAConnect

    /// <summary>
    /// IP to redirect the traffic to, in network order
    /// </summary>
    public static byte[] TITANIC_IP_BE;

    /// <summary>
    /// List of IPs used for Bancho in osu!
    /// </summary>
    private static uint[] originalBanchoIPs =
    {
        1565136690,
        1993635291,
        1778493632,
        3801162414,
        853804760,
        3624330290,
        183079749,
        3416347559,
        1167321354,
        2130706433,
        3624330293,
        3624330292,
        3624330291,
        3624330290,
        167772311,
    };
    
    // Hooked WSAConnect implementation
    private static int HookedWSAConnect(
        IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData, IntPtr lpCaleeData, IntPtr lpSQOS, IntPtr lpGQOS)
    {
        Console.WriteLine("WSAConnect hook triggered");
        
        var sockAddr = (SockaddrIn)Marshal.PtrToStructure(name, typeof(SockaddrIn)); // Get managed struct from native pointer
        if (Array.IndexOf(originalBanchoIPs, sockAddr.sin_addr) < 0)
        {
            Console.WriteLine("Not a Bancho IP, skipping replace");
            return originalWSAConnectFunc(s, name, namelen, lpCallerData, lpCaleeData, lpSQOS, lpGQOS);
        }
        
        sockAddr.sin_addr = BitConverter.ToUInt32(TITANIC_IP_BE, 0); // Replace IP
        
        // Create replaced name
        IntPtr newName = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SockaddrIn)));
        Marshal.StructureToPtr(sockAddr, newName, false);
        
        // Call original WSAConnect
        return originalWSAConnectFunc(s, newName, namelen, lpCallerData, lpCaleeData, lpSQOS, lpGQOS);
    }
    
    public static void Initialize()
    {
        WSAConnectDelegate hookDelegate = HookedWSAConnect; // The function we are replacing with
        IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(hookDelegate); // Get pointer to the function

        IntPtr hWS2 = WinApi.LoadLibrary("ws2_32.dll"); // Load target lib
        IntPtr WSAConnectHandle = WinApi.GetProcAddress(hWS2, "WSAConnect"); // Get the address of function we are hooking
        
        var status = MinHook.MinHook.MH_CreateHook(WSAConnectHandle, hookPtr, out originalWSAConnect); // Create hook
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to create WSAConnect hook {status}");
        }
        
        // Get function from original WSAConnect to call it
        originalWSAConnectFunc = (WSAConnectDelegate)Marshal.GetDelegateForFunctionPointer(originalWSAConnect, typeof(WSAConnectDelegate));
        
        // Do not GC our hooking thingies
        GC.KeepAlive(hookDelegate);
        GC.KeepAlive(originalWSAConnectFunc);
    }
}
