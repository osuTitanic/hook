using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TitanicHook.GUI
{
    public partial class Launcher : Form
    {
        private const string ChangelogUrl = "http://osu.titanic.sh/p/changelog?updater=1";

        public Launcher()
        {
            InitializeComponent();
            Load += OnLoad;
            changelogBrowser.Navigating += OnBrowserNavigating;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            AutoScaleMode = AutoScaleMode.None;
            
            updateProgress.Style = ProgressBarStyle.Marquee;
            extraCheckboxes.Sorted = true;

            try
            {
                changelogBrowser.Navigate(ChangelogUrl);
            }
            catch (Exception ex)
            {
                statusText.Text = "Couldn't load changelog: " + ex.Message;
            }

            // i have honestly no idea what this does but osume included it
            // i'm assuming this is related to internet explorer, and it's navigation sounds
            CoInternetSetFeatureEnabled(
                21, // FEATURE_DISABLE_NAVIGATION_SOUNDS
                2,  // SET_FEATURE_ON_PROCESS
                true
            );
        }

        private void OnBrowserNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.ToString().StartsWith("http://osu.titanic.sh/p/changelog"))
                // continue inside our integrated browser
                return;

            e.Cancel = true;
            Process.Start(e.Url.ToString()); // load external link inside browser
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
