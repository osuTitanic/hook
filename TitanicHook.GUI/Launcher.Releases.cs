using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Text;
using System.Windows.Forms;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private const string ReleasesUrl = "https://api.titanic.sh/releases/";
        private WebClient? releasesRequest;

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
                PopulateReleases(Release.ParseReleases(e.Result));
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

        private void PopulateReleases(Release[] releasesData)
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

        private static void AddRelease(TreeNode category, Release release)
        {
            if (release.IsPreview)
                return;

            TreeNode node = new TreeNode(release.Name);
            node.Tag = release;
            category.Nodes.Add(node);
        }

        private void OnReleaseSelected(object sender, TreeViewEventArgs e)
        {
            Release? release = e.Node.Tag as Release;
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

    }
}
