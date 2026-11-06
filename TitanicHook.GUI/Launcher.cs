using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;
using TitanicHook.Loader;

namespace TitanicHook.GUI
{
    public partial class Launcher : Form
    {
        internal bool LaunchOsu { get; private set; }

        public Launcher()
        {
            InitializeComponent();

#if NET20
            fallbackToggle.Checked = true;
#else
            fallbackToggle.Checked = false;
#endif

            changelogBrowser.Dock = DockStyle.Fill;
            statusText.Visible = true;
            statusText.Text = "Select a version to continue!";
            updateProgress.Visible = true;
            buttonStart.Visible = false;
            buttonStart.Enabled = false;
            selectButton.Enabled = false;

            Load += OnLoad;
            buttonStart.Click += OnStartClick;
            selectButton.Click += OnSelectClick;
            fallbackToggle.CheckedChanged += OnFallbackChanged;
            fallbackToggle.EnabledChanged += (sender, e) => RefreshSelectButton();
            changelogBrowser.Navigating += OnBrowserNavigating;
            tabControl.Selecting += OnTabSelecting;

            UpdateTabState(enabled: false);
        }

        private void OnStartClick(object sender, EventArgs e)
        {
            if (installedOsuPath == null)
            {
                Logging.Error("osu! installation not located");
                buttonStart.Enabled = false;
                return;
            }

            if (!File.Exists(installedOsuPath))
            {
                Logging.Error($"osu! executable not found at '{installedOsuPath}'");
                buttonStart.Enabled = false;
                return;
            }

            LaunchOsu = true;
            Close();
        }

        private void OnLoad(object sender, EventArgs e)
        {
            AutoScaleMode = AutoScaleMode.None;

            updateProgress.Style = ProgressBarStyle.Marquee;
            extraCheckboxes.Sorted = true;

            try
            {
                ClientInstaller.TryRecover(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to recover client: " + ex);
                MessageBox.Show(this, "An previous client could not be recovered: " + ex.Message, "Titanic!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                selectButton.Enabled = false;
                buttonStart.Enabled = false;
                return;
            }

            LoadOsuInstallation();
            LoadReleases();
            LoadChangelog();

#if !DEBUG
            if (!OsuLoader.Config.DisableUpdates)
                CheckForUpdates();
#endif

            // i have honestly no idea what this does but osume included it
            // i'm assuming this is related to internet explorer, and it's navigation sounds
            CoInternetSetFeatureEnabled(
                21, // FEATURE_DISABLE_NAVIGATION_SOUNDS
                2,  // SET_FEATURE_ON_PROCESS
                true
            );
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (installInProgress)
                e.Cancel = true;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (updater != null)
            {
                updater.Dispose();
                updater = null;
            }
            
            if (releasesRequest != null)
            {
                releasesRequest.CancelAsync();
                releasesRequest.Dispose();
                releasesRequest = null;
            }

            if (screenshotRequest != null)
            {
                screenshotRequest.CancelAsync();
                screenshotRequest.Dispose();
                screenshotRequest = null;
            }

            Image image = previewImage.Image;
            previewImage.Image = null;
            if (image != null)
                image.Dispose();

            base.OnFormClosed(e);
        }

        // https://learn.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/ms537168(v=vs.85)
        [DllImport("urlmon.dll")]
        [PreserveSig]
        [return: MarshalAs(UnmanagedType.Error)]
        static extern int CoInternetSetFeatureEnabled(
            int featureEntry,
            [MarshalAs(UnmanagedType.U4)] int dwFlags,
            bool fEnable
        );

    }
}
