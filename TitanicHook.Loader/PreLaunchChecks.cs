using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;
using TitanicHook.Core.Helpers;

namespace TitanicHook.Loader;

public static class PreLaunchChecks
{
    /// <summary>
    /// Checks if requirements are met before proceeding further.
    /// This handles quitting the application too if required.
    /// </summary>
    public static void CheckAll()
    {
        CheckMono();
        CheckFrameworkServicePack();
        CheckOsuAuth();
    }

    private static void CheckMono()
    {
        if (Type.GetType("Mono.Runtime") == null) return;
        
        string netFrameworkRequired = ".NET Framework ";
#if NET20
        netFrameworkRequired += "2.0";
#elif NET40
            netFrameworkRequired += "4.0";
#endif
        string isAllowedString = Program.Config.AllowMono ? "Titanic!Loader will proceed, but might not work as expected" : "Titanic!Loader will now close. Enable AllowMono in configuration file to allow (KEEP IN MIND THAT THIS IS UNSUPPORTED AND WILL CAUSE ISSUES)";
        Logging.LogAndShowError($"Running on Mono is NOT SUPPORTED and WILL CAUSE ISSUES!\n" +
                                $"Please install {netFrameworkRequired} in your Wineprefix to continue.\n" +
                                isAllowedString);
            
        if (!Program.Config.AllowMono)
            Environment.Exit(1);
    }

    private static void CheckFrameworkServicePack()
    {
#if NET20
        // Check is .NET Framework 2.0 SP2 installed.
        // On Windows 2000, officially the latest supported .NET Framework 2.0 service pack is SP1, but
        // its JIT has a bug that will cause a crash when doing some UTF-8 stuff.
        // Service Pack 2 fixes that crash, and despite officially requiring XP, it works on Windows 2000 SP4 too.
        try
        {
            RegistryKey? key =
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v2.0.50727");
            if (key != null)
            {
                int installedSp = (int)key.GetValue("SP");
                if (installedSp < 2)
                {
                    MessageBox.Show(
                        "You don't have .NET Framework 2.0 Service Pack 2 installed.\nIt's required to run Titanic! Hook.\nDownload page for .NET Framework 2.0 SP2 will open.",
                        "Missing update!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    
                    // Link is for 32-bit but probably people running a system without net20 SP2 are running a 32-bit OS anyways
                    Process.Start("http://web.archive.org/web/20090116120000id_/https://download.microsoft.com/download/c/6/e/c6e88215-0178-4c6c-b5f3-158ff77b1f38/NetFx20SP2_x86.exe");
                    Environment.Exit(1);
                }
                key.Close();
            }
        }
        catch (Exception e)
        {
            // Permission issue likely
            Logging.Info($"Ignoring Framework SP check error: {e.Message}");
        }
#endif
    }

    private static void CheckOsuAuth()
    {
        if (File.Exists("osu!auth.dll") && new FileInfo("osu!auth.dll").Length > 0)
        {
            Logging.LogAndShowError("Non-empty osu!auth.dll detected, can't continue!");
            Environment.Exit(1);
        }
    }
}
