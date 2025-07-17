using System;
using System.Runtime.InteropServices;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Hook to specify the codepage as 1252 always to fix UTF-8 BOM issues in score submission
/// Absolutely not sure if this is the correct approach or if it's too low level
/// If it causes issues, we might switch to hooking StringStream ctor to specify correct encoding only there
/// </summary>
public static class ACPHook
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int GetACPDelegate();
    
    private static IntPtr originalGetACP = IntPtr.Zero;
    private static GetACPDelegate originalGetACPFunc;
    
    public static void Initialize()
    {
        GetACPDelegate hookDelegate = HookedGetACP; // The function we are replacing with
        IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(hookDelegate); // Get pointer to the function

        IntPtr kernel32 = WinApi.LoadLibrary("kernel32.dll"); // Load target lib
        IntPtr GetACPHandle = WinApi.GetProcAddress(kernel32, "GetACP"); // Get the address of function we are hooking
        
        var status = MinHook.MinHook.MH_CreateHook(GetACPHandle, hookPtr, out originalGetACP); // Create hook
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to create GetACP hook {status}");
        }
        
        // Get function from original GetACP to call it
        originalGetACPFunc = (GetACPDelegate)Marshal.GetDelegateForFunctionPointer(originalGetACP, typeof(GetACPDelegate));
        
        // Do not GC our hooking thingies
        GC.KeepAlive(hookDelegate);
        GC.KeepAlive(originalGetACPFunc);
    }

    private static int HookedGetACP()
    {
        int acp = originalGetACPFunc();
        if (acp != 1252 && acp != 65001)
        {
            Console.WriteLine("ACP is neither 1252 nor 65001");
        }
        
        // Okay this might be terrible and might have uncontrollable consequences but idk
        return 1252;
    }
}
