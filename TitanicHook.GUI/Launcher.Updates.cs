using System;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private Updater? updater;
        private bool updateInProgress;
        private bool launcherUsesFallback = Environment.Version.Major < 4;
        private bool requestedFallback;
        private bool startAfterUpdate;

        private void CheckForUpdates(bool? useFallback = null, bool autoStartAfterUpdate = false)
        {
            updater?.Dispose();
            requestedFallback = useFallback ?? launcherUsesFallback;
            startAfterUpdate = autoStartAfterUpdate;
            updateInProgress = true;
            RefreshInstallControls();

            statusText.Text = "Checking for updates...";
            updateProgress.Style = ProgressBarStyle.Marquee;
            updateProgress.Visible = true;
            buttonStart.Visible = false;

            updater = new Updater();
            updater.Check(requestedFallback, OnUpdateProgress, OnUpdateCompleted);
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

            try
            {
                statusText.Text = "Installing update...";

                // We don't really need to restart directly
                // Hitting "start osu!" will do the trick as well
                updater!.Install(update, restart: false);
                launcherUsesFallback = requestedFallback;

                DisplayUpdateStatus("Update installed successfully."); // TODO: perhaps display the version
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to install update: " + ex);
                MessageBox.Show(this, "Unable to install the update: " + ex.Message, "Titanic! Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DisplayUpdateStatus("Update installation failed.");
                return;
            }

            if (startAfterUpdate)
                OnStartClick(buttonStart, EventArgs.Empty);
        }

        private void DisplayUpdateStatus(string message)
        {
            updateProgress.Visible = string.IsNullOrEmpty(installedOsuChecksum);
            updateProgress.Value = 0;
            updateProgress.Style = ProgressBarStyle.Marquee;

            buttonStart.Visible = installedOsuPath != null;
            updateInProgress = false;
            RefreshInstallControls();
            statusText.Text = message;
        }
    }
}
