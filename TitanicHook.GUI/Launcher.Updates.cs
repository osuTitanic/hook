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
#if !DEBUG
            statusText.Text = "Checking for updates...";
            updateProgress.Style = ProgressBarStyle.Marquee;
            updateProgress.Visible = true;
            buttonStart.Visible = false;

            updater = new Updater();
            updater.Check(OnUpdateProgress, OnUpdateCompleted);
#endif
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

                statusText.Text = "Update installed successfully."; // TODO: perhaps display the version
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
            updateProgress.Visible = false;
            updateProgress.Value = 0;
            buttonStart.Visible = installedOsuPath != null;
            statusText.Text = message;
        }
    }
}
