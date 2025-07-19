using System;
using System.Runtime.InteropServices;

namespace TitanicHookManaged.Helpers;

/// <summary>
/// Helper for accessing some of the native functions of Windows API
/// </summary>
public static class WinApi
{
    [DllImport("kernel32.dll")]
    public static extern bool AllocConsole();
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string lpFileName);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr FreeLibrary(IntPtr hModule);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(int nStdHandle);
    
    [DllImport("kernel32.dll")]
    public static extern bool SetStdHandle(int nStdHandle, IntPtr handle);
    
    public const int STD_OUTPUT_HANDLE = -11;
    public const int STD_ERROR_HANDLE = -12;
}
