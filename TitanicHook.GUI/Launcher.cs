using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher : Form
    {
        internal bool LaunchOsu { get; private set; }

        public Launcher()
        {
            InitializeComponent();

            changelogBrowser.Dock = DockStyle.Fill;
            statusText.Visible = true;
            statusText.Text = "Select a version to continue!";
            updateProgress.Visible = true;
            buttonStart.Visible = false;
            buttonStart.Enabled = false;

            Load += OnLoad;
            buttonStart.Click += OnStartClick;
            changelogBrowser.Navigating += OnBrowserNavigating;
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

            LoadOsuInstallation();
            LoadReleases();
            LoadChangelog();
            CheckForUpdates();

            // i have honestly no idea what this does but osume included it
            // i'm assuming this is related to internet explorer, and it's navigation sounds
            CoInternetSetFeatureEnabled(
                21, // FEATURE_DISABLE_NAVIGATION_SOUNDS
                2,  // SET_FEATURE_ON_PROCESS
                true
            );
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
