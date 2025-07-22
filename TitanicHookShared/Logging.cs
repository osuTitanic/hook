using System;
using System.Windows.Forms;

namespace TitanicHookShared;

/// <summary>
/// Logging stuff thingy
/// </summary>
public static class Logging // Holy public static void boilerplate class
{
    /// <summary>
    /// Hook is starting to initialize
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookStart(string hookName)
    {
        Console.WriteLine($"[HOOK INIT] {hookName}");
    }
    
    /// <summary>
    /// Hook has initialized
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookDone(string hookName)
    {
        Console.WriteLine($"[HOOK DONE] {hookName}");
    }
    
    /// <summary>
    /// Log steps of a hook
    /// </summary>
    /// <param name="hookName">Hook name</param>
    /// <param name="message">Message</param>
    public static void HookStep(string hookName, string message)
    {
        Console.WriteLine($"[HOOK STEP] ({hookName}): {message}");
    }

    /// <summary>
    /// Hook is patching
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookPatching(string hookName)
    {
        Console.WriteLine($"[HOOK PATCHING] {hookName}");
    }
    
    /// <summary>
    /// Error occured in hooking, show a message box too
    /// </summary>
    /// <param name="hookName">Hook name</param>
    /// <param name="message">Message</param>
    public static void HookError(string hookName, string message)
    {
        Console.WriteLine($"[HOOK ERR] ({hookName}): {message}");
        ShowError($"Hooking error in hook {hookName}\n{message}");
    }

    /// <summary>
    /// Hook has been triggered
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookTrigger(string hookName)
    {
        Console.WriteLine($"[HOOK TRIGGER]: {hookName}");
    }
    
    /// <summary>
    /// Hook wants to talk
    /// </summary>
    /// <param name="hookName">Hook name</param>
    /// <param name="message">Message</param>
    public static void HookOutput(string hookName, string message)
    {
        Console.WriteLine($"[HOOK OUT] ({hookName}): {message}");
    }

    /// <summary>
    /// Normal output, unrelated to hook initialization
    /// </summary>
    /// <param name="message">Message</param>
    public static void Info(string message)
    {
        Console.WriteLine($"[INFO] ({message})");
    }

    /// <summary>
    /// Shows an error message box
    /// </summary>
    /// <param name="message">Message</param>
    public static void ShowError(string message)
    {
        MessageBox.Show(message, "TitanicHook", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Logs and shows an error
    /// </summary>
    /// <param name="message"></param>
    public static void LogAndShowError(string message)
    {
        Console.WriteLine($"[ERROR] ({message})");
        ShowError(message);
    }
}
