using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// This hook will redirect all socket openings to Titanic's server IP
/// TODO: Replace only IPs that peppy used for Bancho
/// </summary>
public class WSAConnectRedirect
{
    // Delegate of WSAConnect
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int WSAConnectDelegate(IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData,
        IntPtr lpCaleeData, IntPtr lpSQOS, IntPtr lpGQOS);
    
    
    private static IntPtr originalWSAConnect = IntPtr.Zero; // Pointer to original WSAConnect
    private static WSAConnectDelegate originalWSAConnectFunc; // Delegate to original WSAConnect

    // Hooked WSAConnect implementation
    private static int HookedWSAConnect(
        IntPtr s, IntPtr name, int namelen, IntPtr lpCallerData, IntPtr lpCaleeData, IntPtr lpSQOS, IntPtr lpGQOS)
    {
        MessageBox.Show("WSAConnect hook triggered", "Hook trigger");
        var sockAddr = (SockaddrIn)Marshal.PtrToStructure(name, typeof(SockaddrIn));
        MessageBox.Show($"Port: {(ushort)IPAddress.NetworkToHostOrder((short)sockAddr.sin_port)}");
        sockAddr.sin_addr = BitConverter.ToUInt32(IPAddress.Parse("207.180.223.46").GetAddressBytes(), 0);
        IntPtr newName = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SockaddrIn)));
        Marshal.StructureToPtr(sockAddr, newName, false);
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
            MessageBox.Show($"Failed to create WSAConnect hook {status}");
        }
        
        // Get function from original WSAConnect to call it
        originalWSAConnectFunc = (WSAConnectDelegate)Marshal.GetDelegateForFunctionPointer(originalWSAConnect, typeof(WSAConnectDelegate));
        
        // Do not GC our hooking thingies
        GC.KeepAlive(hookDelegate);
        GC.KeepAlive(originalWSAConnectFunc);
    }
}
