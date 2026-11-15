using System;
using System.Diagnostics;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private const string ChangelogUrl = "http://osu.titanic.sh/p/changelog?updater=1";

        private void OnBrowserNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString();

            if (url.StartsWith("http://osu.titanic.sh/p/changelog"))
                // continue inside our integrated browser
                return;

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                // we probably can't open that
                return;

            e.Cancel = true;
            Process.Start(e.Url.ToString()); // load external link inside browser
        }

        private void LoadChangelog()
        {
            try
            {
                string targetUrl = ChangelogUrl + (installedOsuVersion != null ? $"&current={installedOsuVersion}" : "");
                Logging.Info($"Browser: Navigating to '{targetUrl}'");
                changelogBrowser.Navigate(targetUrl);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to load changelog: " + ex.Message);
            }
        }

    }
}
