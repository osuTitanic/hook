using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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

            Updater.CleanupOldVersions();
            OsuLoader.Config = new Configuration(configPath);

#if !DEBUG
            // Initialize console if not in debug
            if (OsuLoader.Config.EnableConsole)
                WinApi.InitializeConsole();
#endif

            Logging.UseConsoleLogging = OsuLoader.Config.EnableConsole;
            Logging.UseFileLogging = OsuLoader.Config.LogToFile;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!launchOsu && !FirstRunSafetyCheck(directory, configPath))
                return;

            // pass in -go or disable launcher in config to start osu!
            bool skipLauncher = launchOsu || OsuLoader.Config.DisableLauncher;

            // configure networking before any network requests are made
            ConfigureNetworking();

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

        private static void ConfigureNetworking()
        {
            // set some sane defaults for ServicePoints
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 12;
            ServicePointManager.CheckCertificateRevocationList = false;

            // older frameworks do not define TLS 1.1 and TLS 1.2
            const SecurityProtocolType Tls12 = (SecurityProtocolType)3072;
            const SecurityProtocolType Tls11 = (SecurityProtocolType)768;
            const SecurityProtocolType Tls = SecurityProtocolType.Tls;

            foreach (SecurityProtocolType protocol in new[] { Tls12, Tls11, Tls })
            {
                try
                {
                    Logging.Info($"Attempting to use protocol {protocol}..");
                    ServicePointManager.SecurityProtocol = protocol;
                    Logging.Info($"Success! Using protocol {protocol}");
                    break;
                }
                catch (Exception ex)
                {
                    Logging.Info($"Cannot use protocol {protocol}: {ex}");
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
                    "This folder already contains other files. Installing osu! here may replace existing files.\n\n" +
                    "Create an empty folder for Titanic!, move the launcher into it, then open it again.",
                    "Titanic!", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // create a config file to prevent this warning from showing again
                OsuLoader.Config.SaveConfiguration(OsuLoader.Config.Filename);
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
