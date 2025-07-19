using System;
using System.Runtime.InteropServices;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.MinHook;

namespace TitanicHookManaged.Hooks.Native;

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
        
        var status = MH.CreateHookApi( "shell32", "ShellExecuteExW",hookPtr, out originalShellExecuteExW); // Create hook
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
