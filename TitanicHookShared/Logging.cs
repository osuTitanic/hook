using System;
using System.IO;
using System.Windows.Forms;

namespace TitanicHookShared;

/// <summary>
/// Logging stuff thingy
/// </summary>
public static class Logging // Holy public static void boilerplate class
{
    public static bool UseConsoleLogging = true;

    public static bool UseFileLogging
    {
        get => _useFileLogging;
        set
        {
            _useFileLogging = value;
            if (_useFileLogging)
            {
                // Set up file logging
                _logFileHandle ??= File.OpenWrite(Constants.LogFileName);
                _logFile ??= new StreamWriter(_logFileHandle);
            }
            else
            {
                // Close file logging
                _logFile?.Close();
                _logFileHandle?.Close();
            }
        }
    }

    private static FileStream? _logFileHandle = null;
    private static StreamWriter? _logFile = null;
    private static bool _useFileLogging = false;

    /// <summary>
    /// Internal function to write logs to file and/or console
    /// </summary>
    /// <param name="message"></param>
    private static void WriteToLog(string message)
    {
        // Only do console logging checks in Release
#if !DEBUG
        if (UseConsoleLogging)
#endif
            Console.WriteLine(message);
        
        if (UseFileLogging)
            _logFile?.WriteLine(message);
    }

    /// <summary>
    /// Hook is starting to initialize
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookStart(string hookName)
    {
        WriteToLog($"[HOOK INIT] {hookName}");
    }
    
    /// <summary>
    /// Hook has initialized
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookDone(string hookName)
    {
        WriteToLog($"[HOOK DONE] {hookName}");
    }
    
    /// <summary>
    /// Log steps of a hook
    /// </summary>
    /// <param name="hookName">Hook name</param>
    /// <param name="message">Message</param>
    public static void HookStep(string hookName, string message)
    {
        WriteToLog($"[HOOK STEP] ({hookName}): {message}");
    }

    /// <summary>
    /// Hook is patching
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookPatching(string hookName)
    {
        WriteToLog($"[HOOK PATCHING] {hookName}");
    }

    /// <summary>
    /// Error occured in hooking, show a message box too
    /// </summary>
    /// <param name="hookName">Hook name</param>
    /// <param name="message">Message</param>
    /// <param name="showError">Show error message</param>
    public static void HookError(string hookName, string message, bool showError = true)
    {
        WriteToLog($"[HOOK ERR] ({hookName}): {message}");
        if (showError) ShowError($"Hooking error in hook {hookName}\n{message}");
    }

    /// <summary>
    /// Hook has been triggered
    /// </summary>
    /// <param name="hookName">Hook name</param>
    public static void HookTrigger(string hookName)
    {
        WriteToLog($"[HOOK TRIGGER]: {hookName}");
    }
    
    /// <summary>
    /// Hook wants to talk
    /// </summary>
    /// <param name="hookName">Hook name</param>
    /// <param name="message">Message</param>
    public static void HookOutput(string hookName, string message)
    {
        WriteToLog($"[HOOK OUT] ({hookName}): {message}");
    }

    /// <summary>
    /// Normal output, unrelated to hook initialization
    /// </summary>
    /// <param name="message">Message</param>
    public static void Info(string message)
    {
        WriteToLog($"[INFO] ({message})");
    }

    /// <summary>
    /// Shows an error message box
    /// </summary>
    /// <param name="message">Message</param>
    public static void ShowError(string message)
    {
        MessageBox.Show(message, "Titanic!Loader", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    /// <summary>
    /// Logs and shows an error
    /// </summary>
    /// <param name="message"></param>
    public static void LogAndShowError(string message)
    {
        WriteToLog($"[ERROR] ({message})");
        ShowError(message);
    }
}
