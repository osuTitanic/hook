using System;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private string? installedOsuPath;
        private string? installedOsuChecksum;
        private int? installedOsuVersion;

        private void LoadOsuInstallation()
        {
            installedOsuPath = OsuFileUtils.FindOsuExecutable(AppDomain.CurrentDomain.BaseDirectory);

            if (installedOsuPath == null)
                return;

            tabControl.SelectedTab = tabChangelog; // switch to changelog tab
            buttonStart.Enabled = true;
            buttonStart.Visible = true;
            updateProgress.Visible = false;
            statusText.Text = "All done!";

            Logging.Info($"Found osu! installation: '{installedOsuPath}'");

            installedOsuChecksum = ChecksumUtils.CalculateMd5(installedOsuPath);
            SelectReleaseFromChecksum(installedOsuChecksum);
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

                    if (!release.Matches(checksum))
                        continue;

                    releases.SelectedNode = node;
                    installedOsuVersion = release.Version;
                    Logging.Info($"Selected release: '{release.Name}' ({checksum})");
                    return true;
                }
            }

            Logging.Error($"Could not find any releases with checksum: '{checksum}'");
            return false;
        }

    }
}
