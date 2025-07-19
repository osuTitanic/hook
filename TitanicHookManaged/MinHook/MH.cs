using System;
using System.Runtime.InteropServices;

namespace TitanicHookManaged.MinHook;

/// <summary>
/// Class for interacting with native functions of MinHook
/// </summary>
public static class MH
{
    private const string LIB_NAME = "MinHook.x86.dll";
    
    [DllImport(LIB_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "MH_Initialize")]
    // Initialize the MinHook library. You must call this function EXACTLY ONCE
    // at the beginning of your program.
    public static extern MhStatus Initialize();
    
    [DllImport(LIB_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "MH_CreateHook")]
    // Creates a hook for the specified target function, in disabled state.
    // Parameters:
    //   pTarget     [in]  A pointer to the target function, which will be
    //                     overridden by the detour function.
    //   pDetour     [in]  A pointer to the detour function, which will override
    //                     the target function.
    //   ppOriginal  [out] A pointer to the trampoline function, which will be
    //                     used to call the original target function.
    //                     This parameter can be NULL.
    public static extern MhStatus CreateHook(IntPtr pTarget, IntPtr pDetour, out IntPtr ppOriginal);
    
    [DllImport(LIB_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "MH_CreateHookApi")]
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
    public static extern MhStatus CreateHookApi([MarshalAs(UnmanagedType.LPWStr)] string pszModule, [MarshalAs(UnmanagedType.LPStr)] string pszProcName, IntPtr pDetour, out IntPtr ppOriginal);
    
    [DllImport(LIB_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "MH_EnableHook")]
    // Enables an already created hook.
    // Parameters:
    //   pTarget [in] A pointer to the target function.
    //                If this parameter is MH_ALL_HOOKS, all created hooks are
    //                enabled in one go.
    public static extern MhStatus EnableHook(IntPtr pTarget);
}
