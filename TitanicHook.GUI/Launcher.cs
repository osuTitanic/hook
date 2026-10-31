using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher : Form
    {
        private const string ChangelogUrl = "http://osu.titanic.sh/p/changelog?updater=1";
        private const string ReleasesUrl = "https://api.titanic.sh/releases/";

        private WebClient? screenshotRequest;
        private WebClient? releasesRequest;

        private string? installedOsuPath;
        private string? installedOsuChecksum;
        private int? installedOsuVersion;

        public Launcher()
        {
            InitializeComponent();

            changelogBrowser.Dock = DockStyle.Fill;
            statusText.Visible = true;
            statusText.Text = "Select a version to continue!";
            updateProgress.Visible = true;
            buttonStart.Visible = false;

            Load += OnLoad;
            changelogBrowser.Navigating += OnBrowserNavigating;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            AutoScaleMode = AutoScaleMode.None;

            updateProgress.Style = ProgressBarStyle.Marquee;
            extraCheckboxes.Sorted = true;

            LoadOsuInstallation();
            LoadReleases();
            LoadChangelog();

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

        private void LoadOsuInstallation()
        {
            installedOsuPath = OsuFileUtils.FindOsuExecutable(AppDomain.CurrentDomain.BaseDirectory);

            if (installedOsuPath == null)
                return;

            tabControl.SelectedTab = tabChangelog; // switch to changelog tab
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
                    ClientRelease? release = node.Tag as ClientRelease;

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

        private void LoadReleases()
        {
            releases.Nodes.Clear();

            try
            {
                releasesRequest = new WebClient();
                releasesRequest.Encoding = Encoding.UTF8;
                releasesRequest.DownloadStringCompleted += OnReleasesDownloaded;
                releasesRequest.DownloadStringAsync(new Uri(ReleasesUrl));
            }
            catch (Exception ex)
            {
                if (releasesRequest != null)
                {
                    releasesRequest.Dispose();
                    releasesRequest = null;
                }
                ShowReleaseError(ex.Message);
            }
        }

        private void OnReleasesDownloaded(object sender, DownloadStringCompletedEventArgs e)
        {
            WebClient request = (WebClient)sender;

            if (!object.ReferenceEquals(request, releasesRequest))
            {
                request.Dispose();
                return;
            }

            releasesRequest = null;
            request.Dispose();

            if (IsDisposed || Disposing) return;
            if (e.Cancelled) return;

            if (e.Error != null)
            {
                ShowReleaseError(e.Error.Message);
                return;
            }

            try
            {
                PopulateReleases(ClientRelease.ParseReleases(e.Result));
            }
            catch (Exception ex)
            {
                ShowReleaseError(ex.Message);
            }
        }

        private void ShowReleaseError(string message)
        {
            releases.Nodes.Clear();

            versionLabel.Text = "";
            descriptionLabel.Text = "";
            releasedLabel.Text = "";
            previewText.Text = "Preview unavailable";
            selectButton.Enabled = false;

            Logging.Error("Failed to load releases: " + message);
            MessageBox.Show("Unable to load releases");
        }

        private void PopulateReleases(ClientRelease[] releasesData)
        {
            releases.Nodes.Clear();

            List<string> categories = new List<string>();
            TreeNode recommended = AddCategory("Recommended");

            // Add all recommended releases first
            for (int i = 0; i < releasesData.Length; i++)
                if (releasesData[i].IsRecommended)
                    AddRelease(recommended, releasesData[i]);

            // then proceed with regular categories & their releases
            for (int i = 0; i < releasesData.Length; i++)
            {
                if (releasesData[i].IsPreview)
                    continue;

                string name = releasesData[i].Category;
                if (!categories.Contains(name))
                    categories.Add(name);
            }

            // Sort alphabetically (in reverse)
            categories.Sort((left, right) =>
            {
                int comparison = string.Compare(right, left, StringComparison.OrdinalIgnoreCase);
                return comparison != 0 ? comparison : string.CompareOrdinal(left, right);
            });

            foreach (string name in categories)
            {
                TreeNode category = AddCategory(name);

                for (int i = 0; i < releasesData.Length; i++)
                    if (releasesData[i].Category == name)
                        AddRelease(category, releasesData[i]);
            }

            Logging.Info($"Loaded {categories.Count} categories & {releasesData.Length} total releases");

            // Expand all nodes & select the first release from the recommended section
            releases.ExpandAll();

            if (SelectReleaseFromChecksum(installedOsuChecksum))
                return;

            if (recommended.Nodes.Count > 0)
            {
                releases.SelectedNode = recommended.Nodes[0];
                return;
            }
            foreach (TreeNode category in releases.Nodes)
            {
                if (category.Nodes.Count > 0)
                {
                    releases.SelectedNode = category.Nodes[0];
                    break;
                }
            }
        }

        private TreeNode AddCategory(string name)
        {
            TreeNode category = new TreeNode(name);
            category.NodeFont = new Font("Segoe UI", 8, FontStyle.Bold);
            releases.Nodes.Add(category);
            return category;
        }

        private static void AddRelease(TreeNode category, ClientRelease release)
        {
            if (release.IsPreview)
                return;

            TreeNode node = new TreeNode(release.Name);
            node.Tag = release;
            category.Nodes.Add(node);
        }

        private void OnReleaseSelected(object sender, TreeViewEventArgs e)
        {
            ClientRelease? release = e.Node.Tag as ClientRelease;
            if (release == null)
                return;

            DateTime releasedAt;
            
            releasedLabel.Text = DateTime.TryParse(release.CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out releasedAt)
                ? "released " + releasedAt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)
                : "release date unavailable";

            descriptionLabel.Text = string.IsNullOrEmpty(release.Description)
                ? "No description provided for this build."
                : release.Description;
            
            // TODO: Known bugs / issues warning

            versionLabel.Text = release.Name;

            if (release.ScreenshotUrl != null)
                LoadScreenshot(release.ScreenshotUrl);
        }

        private void LoadScreenshot(string url)
        {
            WebClient? previousRequest = screenshotRequest;
            screenshotRequest = null;
            
            if (previousRequest != null)
            {
                previousRequest.CancelAsync();
                previousRequest.Dispose();
            }

            Image previousImage = previewImage.Image;
            previewImage.Image = null;

            if (previousImage != null)
                previousImage.Dispose();

            previewImage.Visible = false;
            previewText.Visible = true;
            previewText.BringToFront();

            if (string.IsNullOrEmpty(url))
            {
                previewText.Text = "No preview available";
                return;
            }

            Logging.Info($"Downloading screeenshot: '{url}'");
            previewText.Text = "Loading preview ...";

            try
            {
                WebClient request = new WebClient();
                screenshotRequest = request;
                request.DownloadDataCompleted += OnScreenshotDownloaded;
                request.DownloadDataAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                if (screenshotRequest != null)
                {
                    screenshotRequest.Dispose();
                    screenshotRequest = null;
                }
                previewText.Text = "Preview unavailable";
                Logging.Error($"Failed to load screenshot: {ex.Message}");
            }
        }

        private void OnScreenshotDownloaded(object sender, DownloadDataCompletedEventArgs e)
        {
            WebClient request = (WebClient)sender;
            if (!object.ReferenceEquals(request, screenshotRequest))
            {
                // request got replaced
                request.Dispose();
                return;
            }

            screenshotRequest = null;
            request.Dispose();

            if (IsDisposed || Disposing)
                // app is shutting down
                return;

            if (e.Cancelled || e.Error != null)
            {
                previewText.Text = "Preview unavailable";
                return;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(e.Result))
                using (Image image = Image.FromStream(stream))
                    previewImage.Image = new Bitmap(image);

                previewText.Visible = false;
                previewImage.Visible = true;
                previewImage.BringToFront();
            }
            catch (Exception ex)
            {
                previewText.Text = "Preview unavailable";
                Logging.Error($"Failed to display screenshot: {ex.Message}");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
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
