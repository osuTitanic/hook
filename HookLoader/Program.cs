// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TitanicHookManaged;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Hooks;
using TitanicHookManaged.OsuInterop;

namespace HookLoader;

class Program
{
    /// <summary>
    /// Original entry assembly (before hooking)
    /// </summary>
    private static Assembly? _originalEntryAssembly;
    
    public static Configuration Config;
    
    /// <summary>
    /// Small program that will load osu!.exe into memory, execute hooks, and start osu!'s main function
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    static void Main(string[] args)
    {
#if !DEBUG
        // Update check
        Updater.DeleteTempFile();
        Updater.CheckForUpdates();
#endif
        
        if (File.Exists("osu!auth.dll") && new FileInfo("osu!auth.dll").Length > 0)
        {
            Logging.LogAndShowError("Non-empty osu!auth.dll detected, can't continue!");
            return;
        }
        
        Config = new Configuration(Constants.DefaultConfigName);
#if !DEBUG
        // Initialize console if not in debug
        if (Config.EnableConsole)
            WinApi.InitializeConsole();
#endif
        
        Logging.UseConsoleLogging = Config.EnableConsole;
        Logging.UseFileLogging = Config.LogToFile;
        
        if (RunningOnMono)
        {
            string netFrameworkRequired = ".NET Framework ";
#if NET20
            netFrameworkRequired += "2.0";
#elif NET40
            netFrameworkRequired += "4.0";
#endif
            string isAllowedString = Config.AllowMono ? "Titanic!Loader will proceed, but might not work as expected" : "Titanic!Loader will now close. Enable AllowMono in configuration file to allow (KEEP IN MIND THAT THIS IS UNSUPPORTED AND WILL CAUSE ISSUES)";
            Logging.LogAndShowError($"Running on Mono is NOT SUPPORTED and WILL CAUSE ISSUES!\n" +
                                    $"Please install {netFrameworkRequired} in your Wineprefix to continue.\n" +
                                    isAllowedString);
            
            if (!Config.AllowMono)
                return;
        }
        
        _originalEntryAssembly = Assembly.GetEntryAssembly();
        if (_originalEntryAssembly == null)
        {
            Logging.LogAndShowError("Assembly.GetEntryAssembly() returned null");
            return;
        }
        
        // Load osu!
        Assembly loaded;
        try
        {
            OsuPath = FindOsuExecutable(Path.GetDirectoryName(_originalEntryAssembly.Location));
            loaded = Assembly.Load(File.ReadAllBytes(OsuPath));
        }
        catch (Exception e)
        {
            Logging.LogAndShowError($"Failed to load osu!.exe\n {e}");
            return;
        }
        

        if (loaded.ImageRuntimeVersion != Assembly.GetExecutingAssembly().ImageRuntimeVersion)
        {
            Logging.LogAndShowError($".NET Framework runtime version mismatch!\n" +
                                    $"This version of osu! requires .NET Framework {GetShortFrameworkVer(loaded.ImageRuntimeVersion)}, this executable of Titanic!Loader is .NET Framework {GetShortFrameworkVer(Assembly.GetExecutingAssembly().ImageRuntimeVersion)}\n" +
                                    $"Get the correct version of Titanic!Loader for this version of osu!.");
            return;
        }
        
        // Get entry point
        MethodInfo entry = loaded.EntryPoint;
        if (entry == null)
        {
            Logging.LogAndShowError("Entry point not found.");
            return;
        }
        
        List<string> FakeArgs = new();
        FakeArgs.Add(OsuPath);
        if (OsuVersion.GetVersionNumber() >= 20140811)
            FakeArgs.Add("-go"); // -go arg will bypass the updater in older builds. Always adding it won't work, osu! will show unknown file message
        
        // Load hooks specific to the loader
        Logging.Info("Loading early hooks");
        EntryPointHook.Initialize(loaded);
        ExecutablePathHook.Initialize(OsuPath);
        // TODO: find out if it's necessary to force the hook to point at loader's icon
        //ExtractIconHook.Initialize(AppDomain.CurrentDomain.FriendlyName);
        GetArgsHook.Initialize(FakeArgs.ToArray());
        
        // Hook osu!.exe's entrypoint to execute other hooks there
        // This is required because osu!common has to be loaded by osu! for hooking
        // If we would've loaded osu!common manually it wouldn't work
        Logging.Info("Hooking osu!'s main function");
        OsuStartHook.Initialize(entry);
        
        // Start the exe's entry point
        Logging.Info("Starting osu!");
        entry.Invoke(null, new object[] { });
    }

    /// <summary>
    /// Converts long framework version (e.g. v2.0.50727, v4.0.30319) to short (e.g. 2.0, 4.0)
    /// </summary>
    /// <param name="framework">Long framework version</param>
    /// <returns>Short framework version</returns>
    private static string GetShortFrameworkVer(string framework)
    {
        return framework.Substring(1, 4);
    }

    /// <summary>
    /// Finds the osu! executable from a list of valid filenames
    /// </summary>
    /// <param name="directory">Directory to search in</param>
    /// <returns>Path to a first found osu! executable, null if none was found</returns>
    private static string? FindOsuExecutable(string directory)
    {
        string[] validFiles =
            ["osu!.exe", "osu.exe", "osu!test.exe", "osu!public.exe", "osu!shine1.exe", "osu!cuttingedge.exe"];
        return validFiles.Select(file => Path.Combine(directory, file)).FirstOrDefault(File.Exists);
    }

    internal static string OsuPath = "";
    
    private static bool RunningOnMono => Type.GetType("Mono.Runtime") != null;
}
