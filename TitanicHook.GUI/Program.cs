using System;
using System.Diagnostics;
using System.Windows.Forms;
using TitanicHook.Core;
using TitanicHook.Core.Helpers;
using TitanicHook.Loader;

namespace TitanicHook.GUI
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Updater.CleanupOldVersions();
            OsuLoader.Config = new Configuration(Constants.DefaultConfigName);

            // pass in -go or disable launcher in config to start osu!
            bool launchArgument = args.Length > 0 && args[0] == "-go";
            bool skipLauncher = launchArgument || OsuLoader.Config.DisableLauncher;

            if (skipLauncher)
            {
                if (OsuLoader.Config.DisableLauncher && !OsuLoader.Config.DisableUpdates)
                    Updater.CheckUnattended();

                try
                {
                    string? defaultPath = OsuFileUtils.FindOsuExecutable(AppDomain.CurrentDomain.BaseDirectory);
                    string? osuPath = args.Length > 1 ? args[1] : defaultPath;
                    OsuLoader.Start(osuPath!);
                }
                catch (Exception ex)
                {
                    Logging.LogAndShowError("Failed to start osu!: " + ex);
                }
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool launchOsu;
            using (Launcher launcher = new Launcher())
            {
                Application.Run(launcher);
                OsuLoader.Config.ReleaseVersion = launcher.installedOsuVersion ?? OsuLoader.Config.ReleaseVersion;
                OsuLoader.Config.SaveConfiguration(OsuLoader.Config.Filename);
                launchOsu = launcher.LaunchOsu;
            }

            if (launchOsu)
            {
                try
                {
                    Process.Start(Application.ExecutablePath, "-go");
                }
                catch (Exception ex)
                {
                    Logging.LogAndShowError("Failed to start osu!: " + ex);
                }
            }
        }
    }
}
