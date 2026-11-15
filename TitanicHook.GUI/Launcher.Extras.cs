using System;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private WebClient? extrasRequest;
        private bool extrasLoaded;

        private void LoadExtras()
        {
            extrasLoaded = false;
            extraCheckboxes.Items.Clear();
            extrasDescription.Text = "Loading optional packages...";

            try
            {
                extrasRequest = new WebClient { Encoding = Encoding.UTF8 };
                extrasRequest.DownloadStringCompleted += OnExtrasDownloaded;
                extrasRequest.DownloadStringAsync(new Uri(Extra.ReleaseUrl, "update"));
            }
            catch (Exception ex)
            {
                extrasRequest?.Dispose();
                extrasRequest = null;
                ShowExtrasError(ex);
            }
            
            RefreshExtrasState();
        }

        private void OnExtrasDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            WebClient request = (WebClient)sender;

            if (!ReferenceEquals(request, extrasRequest))
            {
                // request got probably replaced
                // we are no longer waiting for it so we can ignore it
                request.Dispose();
                return;
            }
            
            extrasRequest = null;
            request.Dispose();

            if (IsDisposed || Disposing)
                return;

            if (e.Cancelled)
                return;

            try
            {
                if (e.Error != null)
                    throw e.Error;

                string target = AppDomain.CurrentDomain.BaseDirectory;

                foreach (Extra package in Extra.Parse(e.Result, target))
                {
                    if (!package.IsInstalled(target))
                        extraCheckboxes.Items.Add(package);
                }

                extrasLoaded = true;
                RefreshExtraPackages();
            }
            catch (Exception ex)
            {
                extraCheckboxes.Items.Clear();
                ShowExtrasError(ex);
            }

            RefreshExtrasState();
        }

        private void ShowExtrasError(Exception error)
        {
            Logging.Error("Failed to load extras: " + error);
            extrasDescription.Text = "Unable to load optional packages. Click Retry to try again.";
        }

        private void OnExtraItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Run on the next tick to avoid issues with CheckedItems not being updated yet
            // This will run on the UI thread afaik
            BeginInvoke(new MethodInvoker(RefreshExtrasState));
        }

        private void OnInstallExtrasClick(object sender, EventArgs e)
        {
            if (!installExtrasButton.Enabled)
                return;

            if (installInProgress || updateInProgress)
                return;

            if (!extrasLoaded)
            {
                LoadExtras();
                return;
            }

            Extra[] selected = new Extra[extraCheckboxes.CheckedItems.Count];
            extraCheckboxes.CheckedItems.CopyTo(selected, 0);
            OnInstallationBegin();
            statusText.Text = "Installing optional packages...";

            BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };
            worker.DoWork += (s, args) =>
            {
                foreach (Extra package in selected)
                {
                    Installer.InstallExtra(AppDomain.CurrentDomain.BaseDirectory, package,
                        (message, percentage) => worker.ReportProgress(percentage ?? -1, message));
                }
            };
            worker.ProgressChanged += OnInstallationProgressChanged;
            worker.RunWorkerCompleted += (s, args) =>
            {
                worker.Dispose();
                OnExtrasInstallCompleted(args.Error);
            };
            worker.RunWorkerAsync();
        }

        private void OnExtrasInstallCompleted(Exception? error)
        {
            if (IsDisposed || Disposing)
                return;

            installInProgress = false;
            updateProgress.Visible = false;
            buttonStart.Enabled = installedOsuPath != null;
            buttonStart.Visible = buttonStart.Enabled;
            RefreshExtraPackages();
            RefreshInstallControls();

            statusText.Text = error == null ? "Done!" : "Package installation failed.";
            if (error != null)
            {
                Logging.Error("Failed to install extras: " + error);
                MessageBox.Show(
                    this, "Unable to install optional packages: " + error.Message,
                    "Titanic!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshExtraPackages()
        {
            for (int i = extraCheckboxes.Items.Count - 1; i >= 0; i--)
            {
                Extra item = (Extra)extraCheckboxes.Items[i];

                if (item.IsInstalled(AppDomain.CurrentDomain.BaseDirectory))
                    extraCheckboxes.Items.RemoveAt(i);
            }

            if (!extrasLoaded)
                return;
            
            extrasDescription.Text = extraCheckboxes.Items.Count == 0
                ? "No optional packages are available to install."
                : "Select the optional packages you would like to install.";
        }

        private void RefreshExtrasState()
        {
            bool available = installedOsuPath != null && !installInProgress && !updateInProgress;

            extraCheckboxes.Enabled = available && extrasLoaded;
            installExtrasButton.Text = extrasLoaded ? "Install selected" : "Retry";
            installExtrasButton.Enabled = available && extrasRequest == null &&
                (!extrasLoaded || extraCheckboxes.CheckedItems.Count > 0);
        }
    }
}
