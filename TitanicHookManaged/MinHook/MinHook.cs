using System;
using System.Runtime.InteropServices;

namespace TitanicHookManaged.MinHook;

/// <summary>
/// Class for interacting with native functions of MinHook
/// </summary>
public static class MinHook
{
    public const string LIB_NAME = "MinHook.x86.dll";
    
    [DllImport(LIB_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    // Initialize the MinHook library. You must call this function EXACTLY ONCE
    // at the beginning of your program.
    public static extern MhStatus MH_Initialize();
    
    [DllImport(LIB_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
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
    
    [DllImport(LIB_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    // Enables an already created hook.
    // Parameters:
    //   pTarget [in] A pointer to the target function.
    //                If this parameter is MH_ALL_HOOKS, all created hooks are
    //                enabled in one go.
    public static extern MhStatus MH_EnableHook(IntPtr pTarget);
}
