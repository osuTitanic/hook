using System;
using System.Runtime.InteropServices;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Hook to overwrite ppy.sh url opens to titanic.sh
/// </summary>
public static class ShellExecuteHook
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate bool ShellExecuteExWDelegate(ShellExecuteInfo info);
    
    private static IntPtr originalShellExecuteExW = IntPtr.Zero;
    private static ShellExecuteExWDelegate originalShellExecuteExWFunc;
    
    public static void Initialize()
    {
        ShellExecuteExWDelegate hookDelegate = HookedShellExecuteExW; // The function we are replacing with
        IntPtr hookPtr = Marshal.GetFunctionPointerForDelegate(hookDelegate); // Get pointer to the function

        IntPtr shell32 = WinApi.LoadLibrary("shell32.dll"); // Load target lib
        IntPtr ShellExecuteExWhandle = WinApi.GetProcAddress(shell32, "ShellExecuteExW"); // Get the address of function we are hooking
        
        var status = MinHook.MinHook.MH_CreateHook(ShellExecuteExWhandle, hookPtr, out originalShellExecuteExW); // Create hook
        if (status != MhStatus.MH_OK)
        {
            Console.WriteLine($"Failed to create GetACP hook {status}");
        }
        
        // Get function from original ShellExecuteExW to call it
        originalShellExecuteExWFunc = (ShellExecuteExWDelegate)Marshal.GetDelegateForFunctionPointer(originalShellExecuteExW, typeof(ShellExecuteExWDelegate));
        
        // Do not GC our hooking thingies
        GC.KeepAlive(hookDelegate);
        GC.KeepAlive(originalShellExecuteExWFunc);
    }

    private static bool HookedShellExecuteExW(ShellExecuteInfo info)
    {
        Console.WriteLine("ShellExecuteExW hook called");
        
        if (info.lpFile.Contains("ppy.sh"))
        {
            Console.WriteLine("Replacing domain in ShellExecuteExW");
            info.lpFile = info.lpFile.Replace("ppy.sh", "titanic.sh"); // I hope there isn't a memory leak here...
        }
            
        return originalShellExecuteExWFunc(info);
    }
}
