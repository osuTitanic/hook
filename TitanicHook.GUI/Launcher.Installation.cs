using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;
using TitanicHook.Loader;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private bool installInProgress;
        internal string? installedOsuPath;
        internal string? installedOsuChecksum;
        internal int? installedOsuVersion;

        private void OnSelectClick(object sender, EventArgs e)
        {
            Release? release = releases.SelectedNode?.Tag as Release;

            if (release == null)
                return;

            if (!selectButton.Enabled)
                return;

            OnInstallationBegin(); // update the ui to show that installation is in progress

            BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
            // args.Result -> whether client is fallback or not
            worker.DoWork += (s, args) => args.Result = Installer.InstallRelease(
                AppDomain.CurrentDomain.BaseDirectory, release,
                (message, percentage) => worker.ReportProgress(percentage ?? -1, message)
            );
            worker.ProgressChanged += OnInstallationProgressChanged;
            worker.RunWorkerCompleted += (s, args) =>
            {
                worker.Dispose();
                CompleteClientInstall(release, args.Error, args.Error == null && (bool)args.Result);
            };
            worker.RunWorkerAsync();
        }

        private void CompleteClientInstall(Release release, Exception? error, bool useFallback)
        {
            if (IsDisposed || Disposing)
                return;

            installInProgress = false;
            updateProgress.Visible = false;
            RefreshInstallControls();

            if (error != null)
            {
                buttonStart.Enabled = installedOsuPath != null && File.Exists(installedOsuPath);
                buttonStart.Visible = buttonStart.Enabled;
                
                Logging.Error("Failed to install client: " + error);
                statusText.Text = "Installation failed.";
                MessageBox.Show(this, "Unable to install client: " + error.Message, "Titanic!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LoadOsuInstallation(release.Version);
            RefreshExtraPackages();
            LoadChangelog();
            statusText.Text = "Client installed successfully.";

            // Set config back to its default so that hook can adjust
            OsuLoader.Config.ClientSha256 = "";
            OsuLoader.Config.ReleaseVersion = release.Version;
            OsuLoader.Config.HookTcpConnections = true; // TODO: We can infer that from the release version i think
            OsuLoader.Config.HookNetLibHeaders = true;
            OsuLoader.Config.HookNetLibEncoding = true;
            OsuLoader.Config.HookModernHostMethod = !useFallback;
            OsuLoader.Config.HookCheckCertificate = !useFallback;

            // TODO: Remove settings from osu!.<username>.cfg that are not safe to transfer between versions
            //       Encountered some issues when switching from e.g. b20141106.14 to b1818

            // Check if both launcher & client are on the same framework version
            if (useFallback != launcherUsesFallback)
            {
                if (!OsuLoader.Config.DisableUpdates)
                {
                    CheckForUpdates(useFallback, autoStart.Checked);
                    return;
                }

                // Updates are disabled :(
                // Let's tell the user that they should do it instead
                MessageBox.Show(
                    this,
                    "This client uses a different .NET Framework version. " +
                    "If you experience issues, please download the 'fallback' version of Titanic.",
                    "Titanic!", MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
            }

            if (autoStart.Checked)
                OnStartClick(buttonStart, EventArgs.Empty);
        }

        private void LoadOsuInstallation(int? version = null)
        {
            installedOsuPath = OsuFileUtils.FindOsuExecutable(AppDomain.CurrentDomain.BaseDirectory);

            if (installedOsuPath == null)
                return;

            UpdateTabState(enabled: true); // show changelog & extra tabs
            tabControl.SelectedTab = tabChangelog; // switch to changelog tab
            buttonStart.Enabled = true;
            buttonStart.Visible = true;
            updateProgress.Visible = false;
            statusText.Text = "All done!";

            Logging.Info($"Found osu! installation: '{installedOsuPath}'");

            installedOsuChecksum = ChecksumUtils.CalculateMd5(installedOsuPath);
            installedOsuVersion = version;
            SelectReleaseFromChecksum(installedOsuChecksum);
            RefreshInstallControls();
        }

        private bool SelectReleaseFromChecksum(string? checksum)
        {
            if (string.IsNullOrEmpty(checksum))
                return false;

            if (releases.Nodes.Count == 0)
                // releases have not been loaded yet
                return false;

            foreach (TreeNode category in releases.Nodes)
            {
                foreach (TreeNode node in category.Nodes)
                {
                    Release? release = node.Tag as Release;

                    if (release == null)
                        continue;

                    if (!release.Matches(checksum!))
                        continue;

                    installedOsuVersion = release.Version;
                    releases.SelectedNode = node;
                    RefreshSelectButton();
                    Logging.Info($"Selected release: '{release.Name}' ({checksum})");
                    return true;
                }
            }

            Logging.Error($"Could not find any releases with checksum: '{checksum}'");
            return false;
        }

        private void UpdateTabState(bool enabled)
        {
            tabChangelog.Enabled = enabled;
            tabExtra.Enabled = enabled;

            if (!enabled)
                tabControl.SelectedTab = tabSelect;
        }

        private void OnTabSelecting(object sender, TabControlCancelEventArgs e)
        {
            if (installedOsuPath != null)
                return;

            if (e.TabPage != tabSelect)
                // user has not installed a version of osu! yet
                // so we cannot show them the changelog or extra tabs
                e.Cancel = true;
        }

        private void OnInstallationBegin()
        {
            installInProgress = true;
            RefreshInstallControls();
            buttonStart.Enabled = false;
            buttonStart.Visible = false;
            updateProgress.Style = ProgressBarStyle.Marquee;
            updateProgress.Visible = true;
        }

        private void OnInstallationProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (IsDisposed || Disposing)
                return;

            statusText.Text = (string)e.UserState;
            updateProgress.Style = e.ProgressPercentage < 0 ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;

            if (e.ProgressPercentage >= 0)
                updateProgress.Value = e.ProgressPercentage;
        }

        private void RefreshInstallControls()
        {
            RefreshSelectButton();
            RefreshExtrasState();
        }
    }
}
