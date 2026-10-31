using System;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private Updater? updater;

        private void CheckForUpdates()
        {
            updater?.Dispose();
            statusText.Text = "Checking for updates...";
            updateProgress.Style = ProgressBarStyle.Marquee;
            updateProgress.Visible = true;
            buttonStart.Visible = false;
            fallbackToggle.Enabled = false;

            updater = new Updater();
            updater.Check(fallbackToggle.Checked, OnUpdateProgress, OnUpdateCompleted);
        }

        private void OnFallbackChanged(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void OnUpdateProgress(int? percentage)
        {
            if (IsDisposed || Disposing)
                return;

            if (!percentage.HasValue)
                return;

            if (updateProgress.Style != ProgressBarStyle.Continuous)
            {
                updateProgress.Style = ProgressBarStyle.Continuous;
                statusText.Text = "Downloading update...";
            }

            updateProgress.Value = percentage.Value;
        }

        private void OnUpdateCompleted(byte[]? update, Exception? error)
        {
            if (IsDisposed || Disposing)
                return;

            if (error != null)
            {
                Logging.Error("Failed to check for updates: " + error);
                DisplayUpdateStatus("Update check failed.");
                return;
            }

            if (update == null || update.Length == 0)
            {
                DisplayUpdateStatus("Titanic! is up to date.");
                return;
            }

            DisplayUpdateStatus("A Titanic! update is available.");

            try
            {
                statusText.Text = "Installing update...";

                // We don't really need to restart directly
                // Hitting "start osu!" will do the trick as well
                Updater.Install(update, false);

                DisplayUpdateStatus("Update installed successfully."); // TODO: perhaps display the version
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to install update: " + ex);
                MessageBox.Show(this, "Unable to install the update: " + ex.Message, "Titanic! Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisplayUpdateStatus("Update installation failed.");
            }
        }

        private void DisplayUpdateStatus(string message)
        {
            updateProgress.Visible = string.IsNullOrEmpty(installedOsuChecksum);
            updateProgress.Value = 0;
            updateProgress.Style = ProgressBarStyle.Marquee;

            buttonStart.Visible = installedOsuPath != null;
            fallbackToggle.Enabled = true;
            statusText.Text = message;
        }
    }
}
