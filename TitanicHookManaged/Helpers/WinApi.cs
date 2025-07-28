using System;
using System.IO;
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
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    public static extern uint GetACP();
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string lpFileName);
    
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    
    public const int STD_OUTPUT_HANDLE = -11;
    public const int STD_ERROR_HANDLE = -12;

    /// <summary>
    /// Initializes the console after injecting
    /// </summary>
    public static void InitializeConsole()
    {
        if (!AllocConsole())
            return; // Console already initialized
        
        var stdout = GetStdHandle(STD_OUTPUT_HANDLE);
        var stderr = GetStdHandle(STD_ERROR_HANDLE);
        var outFs = new FileStream(stdout, FileAccess.Write);
        var outWriter = new StreamWriter(outFs)
        {
            AutoFlush = true
        };
        var errFs = new FileStream(stderr, FileAccess.Write);
        var errWriter = new StreamWriter(errFs)
        {
            AutoFlush = true
        };
        Console.SetOut(outWriter);
        Console.SetError(errWriter);
    }

    /// <summary>
    /// Simple check for Wine
    /// </summary>
    public static bool RunningUnderWine
    {
        get
        {
            IntPtr ntdllHandle = LoadLibrary("ntdll.dll");
            if (ntdllHandle == IntPtr.Zero)
                return false;
        
            IntPtr wineFunc = GetProcAddress(ntdllHandle, "wine_get_version");
            return wineFunc != IntPtr.Zero;
        }
    }
}
