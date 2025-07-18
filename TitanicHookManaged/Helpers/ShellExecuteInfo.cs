using System;
using System.Runtime.InteropServices;

namespace TitanicHookManaged.Helpers;

[StructLayout(LayoutKind.Sequential)]
public class ShellExecuteInfo
{
    public int cbSize;
    public int fMask;
    public IntPtr hwnd = (IntPtr)0;
    public IntPtr lpVerb = (IntPtr)0;
    
    [MarshalAs(UnmanagedType.LPWStr)]
    public string lpFile = "";
    
    public IntPtr lpParameters = (IntPtr)0;
    public IntPtr lpDirectory = (IntPtr)0;
    public int nShow;
    public IntPtr hInstApp = (IntPtr)0;
    public IntPtr lpIDList = (IntPtr)0;
    public IntPtr lpClass = (IntPtr)0;
    public IntPtr hkeyClass = (IntPtr)0;
    public int dwHotKey;
    public IntPtr hIcon = (IntPtr)0;
    public IntPtr hProcess = (IntPtr)0;
}
