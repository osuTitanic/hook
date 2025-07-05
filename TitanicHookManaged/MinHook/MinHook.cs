using System;
using System.Runtime.InteropServices;

namespace TitanicHookManaged.MinHook;

public static class MinHook
{
    [DllImport("MinHook.x86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    // Initialize the MinHook library. You must call this function EXACTLY ONCE
    // at the beginning of your program.
    public static extern MhStatus MH_Initialize();
    
    [DllImport("MinHook.x86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    // Creates a hook for the specified target function, in disabled state.
    // Parameters:
    //   pTarget     [in]  A pointer to the target function, which will be
    //                     overridden by the detour function.
    //   pDetour     [in]  A pointer to the detour function, which will override
    //                     the target function.
    //   ppOriginal  [out] A pointer to the trampoline function, which will be
    //                     used to call the original target function.
    //                     This parameter can be NULL.
    public static extern MhStatus MH_CreateHook(IntPtr pTarget, IntPtr pDetour, out IntPtr ppOriginal);
    
    [DllImport("MinHook.x86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    // Creates a hook for the specified API function, in disabled state.
    // Parameters:
    //   pszModule   [in]  A pointer to the loaded module name which contains the
    //                     target function.
    //   pszProcName [in]  A pointer to the target function name, which will be
    //                     overridden by the detour function.
    //   pDetour     [in]  A pointer to the detour function, which will override
    //                     the target function.
    //   ppOriginal  [out] A pointer to the trampoline function, which will be
    //                     used to call the original target function.
    //                     This parameter can be NULL.
    public static extern MhStatus MH_CreateHookApi([MarshalAs(UnmanagedType.LPWStr)] string pszModule, [MarshalAs(UnmanagedType.LPWStr)] string pszProcName, IntPtr pDetour, out IntPtr ppOriginal);
    
    [DllImport("MinHook.x86.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    // Enables an already created hook.
    // Parameters:
    //   pTarget [in] A pointer to the target function.
    //                If this parameter is MH_ALL_HOOKS, all created hooks are
    //                enabled in one go.
    public static extern MhStatus MH_EnableHook(IntPtr pTarget);
}
