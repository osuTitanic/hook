using System;
using System.Runtime.InteropServices;

namespace TitanicHookManaged;

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
}
