using System;
using System.Diagnostics;
using System.IO;
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
            string directory = AppDomain.CurrentDomain.BaseDirectory;
            string configPath = Path.Combine(directory, Constants.DefaultConfigName);
            bool launchOsu = args.Length > 0 && args[0] == "-go";

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!launchOsu && !FirstRunSafetyCheck(directory, configPath))
                return;

            Updater.CleanupOldVersions();
            OsuLoader.Config = new Configuration(configPath);

            // pass in -go or disable launcher in config to start osu!
            bool skipLauncher = launchOsu || OsuLoader.Config.DisableLauncher;

            if (skipLauncher)
            {
                if (OsuLoader.Config.DisableLauncher && !OsuLoader.Config.DisableUpdates)
                    Updater.CheckUnattended();

                try
                {
                    string? defaultPath = OsuFileUtils.FindOsuExecutable(directory);
                    string? osuPath = args.Length > 1 ? args[1] : defaultPath;
                    OsuLoader.Start(osuPath!);
                }
                catch (Exception ex)
                {
                    Logging.LogAndShowError("Failed to start osu!: " + ex);
                }
                return;
            }

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

        private static bool FirstRunSafetyCheck(string directory, string configPath)
        {
            try
            {
                string launcherName = Path.GetFileName(Application.ExecutablePath);
                
                if (File.Exists(configPath))
                    return true;

                if (OsuFileUtils.FindOsuExecutable(directory) != null)
                    // we'll assume that the user knows what they're doing if osu! is already installed
                    return true;

                bool hasOtherFiles = Array.Exists(Directory.GetFiles(directory), path =>
                {
                    string name = Path.GetFileName(path);
                    return !string.Equals(name, launcherName, StringComparison.OrdinalIgnoreCase) &&
                           !string.Equals(name, Constants.LogFileName, StringComparison.OrdinalIgnoreCase);
                });

                if (!hasOtherFiles)
                    return true;

                MessageBox.Show(
                    "This folder already contains other files or folders. " +
                    "Installing osu! here may replace existing files.\n\n" +
                    "Create an empty folder for Titanic!, move this launcher into it, then open it again.",
                    "Titanic!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (IOException ex)
            {
                Logging.LogAndShowError("Could not prepare the Titanic! folder: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logging.LogAndShowError("Could not prepare the Titanic! folder: " + ex.Message);
            }
            return false;
        }
    }
}
