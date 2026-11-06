using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

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

            installInProgress = true;
            selectButton.Enabled = false;
            buttonStart.Enabled = false;
            buttonStart.Visible = false;
            fallbackToggle.Enabled = false;
            updateProgress.Visible = true;
            updateProgress.Style = ProgressBarStyle.Marquee;

            BackgroundWorker worker = new BackgroundWorker{WorkerReportsProgress=true};

            worker.DoWork += (s, args) => ClientInstaller.Install(
                AppDomain.CurrentDomain.BaseDirectory, release,
                (message, percentage) => worker.ReportProgress(percentage ?? -1, message)
            );
            worker.ProgressChanged += (s, args) =>
            {
                if (IsDisposed || Disposing)
                    return;

                statusText.Text = (string)args.UserState;
                updateProgress.Style = args.ProgressPercentage < 0 ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;

                if (args.ProgressPercentage >= 0)
                    updateProgress.Value = args.ProgressPercentage;
            };
            worker.RunWorkerCompleted += (s, args) =>
            {
                worker.Dispose();
                CompleteClientInstall(release, args.Error);
            };
            worker.RunWorkerAsync();
        }

        private void CompleteClientInstall(Release release, Exception? error)
        {
            if (IsDisposed || Disposing)
                return;

            installInProgress = false;
            fallbackToggle.Enabled = true;
            updateProgress.Visible = false;

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
            LoadChangelog();
            statusText.Text = "Client installed successfully.";

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
            RefreshSelectButton();
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
    }
}
