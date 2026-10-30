using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TitanicHook.GUI
{
    public partial class Launcher : Form
    {
        private const string ChangelogUrl = "http://osu.titanic.sh/p/changelog?updater=1";
        private WebClient screenshotRequest;

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
            PopulateReleases();

            updateProgress.Style = ProgressBarStyle.Marquee;
            extraCheckboxes.Sorted = true;

            try
            {
                changelogBrowser.Navigate(ChangelogUrl);
            }
            catch (Exception ex)
            {
                statusText.Visible = true;
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

        private void PopulateReleases()
        {
            releases.Nodes.Clear();

            List<string> categories = new List<string>();
            ClientRelease[] releasesData = ReleaseCatalog.All;
            TreeNode recommended = AddCategory("Recommended");

            // Add all recommended releases first
            for (int i = 0; i < releasesData.Length; i++)
                if (releasesData[i].IsRecommended)
                    AddRelease(recommended, releasesData[i]);

            // then proceed with regular categories & their releases
            for (int i = 0; i < releasesData.Length; i++)
            {
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

            // Expand all nodes & select the first release from the recommended section
            releases.ExpandAll();
            if (recommended.Nodes.Count > 0)
                releases.SelectedNode = recommended.Nodes[0];
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
            ClientRelease release = e.Node.Tag as ClientRelease;
            if (release == null)
                return;

            DateTime releasedAt;
            
            releasedLabel.Text = DateTime.TryParse(release.CreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out releasedAt)
                ? "released " + releasedAt.ToString("MMM d, yyyy", CultureInfo.InvariantCulture)
                : "release date unavailable";

            descriptionLabel.Text = string.IsNullOrEmpty(release.Description)
                ? "No description provided for this build."
                : release.Description;

            versionLabel.Text = release.Name;
            
            // TODO: Known bugs / issues warning
            LoadScreenshot(release.ScreenshotUrl);
        }

        private void LoadScreenshot(string url)
        {
            WebClient previousRequest = screenshotRequest;
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

            previewText.Text = "Loading preview ...";
            try
            {
                WebClient request = new WebClient();
                screenshotRequest = request;
                request.DownloadDataCompleted += OnScreenshotDownloaded;
                request.DownloadDataAsync(new Uri(url));
            }
            catch (Exception)
            {
                if (screenshotRequest != null)
                {
                    screenshotRequest.Dispose();
                    screenshotRequest = null;
                }
                previewText.Text = "Preview unavailable";
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
            catch (Exception)
            {
                previewText.Text = "Preview unavailable";
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
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

        private sealed class ClientRelease
        {
            public readonly string Name;
            public readonly string Category;
            public readonly string Description;
            public readonly string KnownBugs;
            public readonly bool IsPreview;
            public readonly bool IsRecommended;
            public readonly string DownloadUrl;
            public readonly string[] Checksums;
            public readonly string ScreenshotPath;
            public readonly string CreatedAt;

            public ClientRelease(string name, string category, string description, string knownBugs,
                bool isPreview, bool isRecommended, string downloadUrl, string[] checksums,
                string screenshotPath, string createdAt)
            {
                Name = name;
                Category = category;
                Description = description;
                KnownBugs = knownBugs;
                IsPreview = isPreview;
                IsRecommended = isRecommended;
                DownloadUrl = downloadUrl;
                Checksums = checksums;
                ScreenshotPath = screenshotPath;
                CreatedAt = createdAt;
            }

            public string ScreenshotUrl
            {
                get { return "https://osu.titanic.sh" + ScreenshotPath; }
            }
        }

        private static class ReleaseCatalog
        {
            // TODO: Load from titanic api
            public static readonly ClientRelease[] All = new ClientRelease[]
            {
                new ClientRelease("b20151228.3fallback", "2015", "Released right after Christmas", null, false, false, "https://cdn.titanic.sh/clients/b20151228.3.zip", new string[] { "fbe80082a80bbb1c8c338db2918e5b21", "c4a037a674085795be7874a038c6f37c", "14ac386d7e9dbf7b9196c7c446f66eca" }, "/images/clients/small/b20151228.3.jpg", "2025-03-25T21:25:49.678747"),
                new ClientRelease("b20151128.2fallback", "2015", "", null, true, false, "https://cdn.titanic.sh/clients/b20151128.2fallback.zip", new string[] { "7d5cde9324671ad20452ba470e0031f5", "4fe1deb7a063e66a99edbf25f45a80d3" }, "/images/clients/small/b20151128.2fallback.jpg", "2025-03-25T21:24:56.875010"),
                new ClientRelease("b20151026.3fallback", "2015", "Adds the ability to disable the startup splash screen with \"-nosplash\"", null, false, false, "https://cdn.titanic.sh/clients/b20151026.3fallback.zip", new string[] { "a8c42894dbb5b6e8a8cb03d86e8fd695", "aebb1c7fd89c13fb0ed7c3e2a4c7c0b8", "3580637429ae991322b726c59505b214" }, "/images/clients/small/b20151026.3fallback.jpg", "2025-03-25T21:24:10.270829"),
                new ClientRelease("b20150901", "2015", "This version adds a new design for the tournament client", null, false, false, "https://cdn.titanic.sh/clients/b20150901.zip", new string[] { "8ce6385b15a2e94b8e4d20af206f1d6e", "4b31395587c08977f4979b628b22cc19", "f46a185e68858b722f77b05ae041e421" }, "/images/clients/small/b20150901.jpg", "2025-03-25T21:23:02.558599"),
                new ClientRelease("b20150826.3", "2015", "This version now displays the qualified warning message", null, false, false, "https://cdn.titanic.sh/clients/b20150826.3.zip", new string[] { "8ba13de4b986b1706edf9c8935704feb", "55c560df16620ebc3340e1b007cce95b", "e0a67dd456e20b297952a1cae8118c0d" }, "/images/clients/small/b20150826.3.jpg", "2025-03-25T21:21:49.454301"),
                new ClientRelease("b20150812.11", "2015", "Added support for osu://spectate/<user> links", null, false, false, "https://cdn.titanic.sh/clients/b20150812.11.zip", new string[] { "7b495e3c189e69dc69931cc91754693a", "c598e4637d2a638506ee2834f54b8e9e", "b5c621c987a4f6ef5e0825d12ce091e7" }, "/images/clients/small/b20150812.11.jpg", "2025-03-25T21:18:55.701059"),
                new ClientRelease("b20150525.4cuttingedge", "2015", "Made some improvements to the tournament client", null, true, false, "https://cdn.titanic.sh/clients/b20150525.4cuttingedge.zip", new string[] { "4234ad5d7db89213edc00f1bef1172a1", "cb79b154eca758bcf349b7c7ad8bdae2" }, "/images/clients/small/b20150525.4cuttingedge.jpg", "2025-03-25T21:17:23.385620"),
                new ClientRelease("b20150514.8cuttingedge", "2015", "Added \"My Love\" and \"Lost\" to the bundled beatmaps", null, false, false, "https://cdn.titanic.sh/clients/b20150514.8cuttingedge.zip", new string[] { "cb54c4b4bab5a36e2b01974934ea3057", "ef2ecfae22e1ceadf7b0a5d74ea62f30" }, "/images/clients/small/b20150514.8cuttingedge.jpg", "2025-02-23T19:40:37.487141"),
                new ClientRelease("b20150416.1cuttingedge", "2015", "Allows for adding/removing entire sets in collections", null, false, false, "https://cdn.titanic.sh/clients/b20150416.1cuttingedge.zip", new string[] { "770b38f8ce24dfc0e38273567263ed0f", "e46cb5e6a2676015e66519a10ad4d9c2", "236363c4eb3f91b53b6eaf11e7423bb0" }, "/images/clients/small/b20150416.1cuttingedge.jpg", "2025-03-25T21:13:04.220953"),
                new ClientRelease("b20150331.2", "2015", "Buy the coins!", null, false, false, "https://cdn.titanic.sh/clients/b20150331.2.zip", new string[] { "3be57a84ea1dde2c3e4d30a7837f884c", "d607860f612a62ead5c089ed1921e747", "160648b469c39dd9bc3be5e6345bf322" }, "/images/clients/small/b20150331.2.jpg", "2025-03-25T22:37:54.122726"),
                new ClientRelease("b20150331cuttingedge", "2015", "Buy the coins! (Triangles Edition)", null, false, false, "https://cdn.titanic.sh/clients/b20150331cuttingedge.zip", new string[] { "f305acaf35726903f03ba137a3b6ed4f", "fa7bdd4dd711eb98b55747650318b290", "f79339452804a1e476a40b35f6ab1ad5" }, "/images/clients/small/b20150331cuttingedge.jpg", "2025-03-25T22:43:47.592594"),
                new ClientRelease("b20150205cuttingedge", "2015", "An experimental version from 2015 that disables the DirectX renderer and switches to .NET 4", "This client only works in windowed or borderless mode", false, false, "https://cdn.titanic.sh/clients/b20150205cuttingedge.zip", new string[] { "bdcc4afb920290133f46c27c28f29de2", "4e715a04a8a5f0a1123b677835bad009" }, "/images/clients/small/b20150205cuttingedge.jpg", "2024-12-30T16:34:33.570908"),
                new ClientRelease("b20150203.7", "2015", "Added the new volume controls", null, false, false, "https://cdn.titanic.sh/clients/b20150203.7.zip", new string[] { "55d8a1518cfa7fab0bb1eb20f0f02d47", "646b4e8084f60ac837c147404c5e2301" }, "/images/clients/small/b20150203.7.jpg", "2024-12-30T16:59:55.234698"),
                new ClientRelease("b20150101cuttingedge", "2015", "First release of 2015", null, false, false, "https://cdn.titanic.sh/clients/b20150101cuttingedge.zip", new string[] { "e367b3793980362e3c07bad03e7720a5", "fd2653ae20fc0e79bb8a74c3562b420f", "307a4d6b6d61e92c82597840251d55e5" }, "/images/clients/small/b20150101cuttingedge.jpg", "2024-12-30T16:37:15.044609"),
                new ClientRelease("b20141222.6cuttingedge", "2014", "A christmas-themed version from 2014", null, false, false, "https://cdn.titanic.sh/clients/b20141222.6cuttingedge.zip", new string[] { "c806ce351ab929f99bfdb408c729a3b0", "5daa215bce01e20127e490d2fb667fae", "ec96b168ffed908f382fc0caa3b92bfd", "edb6a946fcddd873f4b70cc5d1bf7281", "29c65118efcb47f8b02f2f4fa2572b84", "2b7c650d6feba4f2378febf673c37fae" }, "/images/clients/small/b20141226.06ce.jpg", "2024-05-03T14:12:30.979295"),
                new ClientRelease("b20141206cuttingedge", "2014", "A 2014 triangles build, with a little easter egg", null, false, false, "https://cdn.titanic.sh/clients/b20141206cuttingedge.zip", new string[] { "7374bc0f71ae6890b6a8bd65b8191f0d", "2a7aa0f279ab630c4beb073fbbd292c9", "348ca9b79908d7e29e6ae1056763f74d", "fb54ab6fd5009f48de1d593ac60517eb", "0760fbeac366f847da75c35194ed4f84", "d9e5b7d7dc27be310e5461692bc34bea" }, "/images/clients/small/b20141206ce.jpg", "2024-05-03T14:12:30.999610"),
                new ClientRelease("b20141109.14cuttingedge", "2014", "A version from 2014 with customizable triangles! (Try to drag an image into the game)", null, false, false, "https://cdn.titanic.sh/clients/b20141109.14cuttingedge.zip", new string[] { "d37e35186fe843c89d6cc938bf9af7c3", "863bd46c02bc26127ac65b69d2bf0f2e", "01223afdf5a65f4b7c5ac98ec35926bb", "e93d18b5ac00811e19c372bdd271fce0", "4fb7fc66a4a5c3beaf2ab2de526c23ff" }, "/images/clients/small/b20141109.14cuttingedge.jpg", "2024-05-03T14:12:31.020466"),
                new ClientRelease("b20141106.14", "2014", "Yet another version from 2014", null, false, true, "https://cdn.titanic.sh/clients/b20141106.14.zip", new string[] { "cdde1ece682c03f402e336409f97b0b5", "9ae19358bf2e731ee8914c296d5b539d", "74b9a885d0a3873be6c96e3a28a1f22a", "5e7b3f960d62fde80afe5348986cfb56", "5135069b5591e9e510ba9abcca61aa44" }, "/images/clients/small/b20141106.14.jpg", "2024-06-29T01:57:07.674729"),
                new ClientRelease("b20140909.11cuttingedge", "2014", "A test build from late 2014, with a very nice UI", null, false, false, "https://cdn.titanic.sh/clients/b20140909.11cuttingedge.zip", new string[] {  }, "/images/clients/small/b20140909.11cuttingedge.jpg", "2024-05-03T14:12:31.041644"),
                new ClientRelease("b20140909.7cuttingedge", "2014", "The same version as b20140909.11cuttingedge, without the UI update", null, false, false, "https://cdn.titanic.sh/clients/b20140909.7cuttingedge.zip", new string[] { "2492a85829d7f81081929e753a28e080", "d6a0d802264e190fc26792198dfa238e", "411d0a4ddb69a3482d1e2de5b3687c8d", "e655ecc7d6d007e65b5aa231fa8f9012", "091b7ddfee0fe6680345ebe2dce8556e" }, "/images/clients/small/b20140909.7cuttingedge.jpg", "2024-05-03T14:12:31.062595"),
                new ClientRelease("b20140616.test", "2014", "One of the earliest versions to support decimal AR, OD, CS values", null, false, false, "https://cdn.titanic.sh/clients/b20140616.test.zip", new string[] { "b5c4881577650526cccd625c257b7a4c", "d8beed3aef416050c386fdfb8df15610", "c586e5e96e8533cf8af053259af773e1", "83526435e59a66ac3bb0f38670773710" }, "/images/clients/small/b20140616.test.jpg", "2024-05-03T14:12:31.083542"),
                new ClientRelease("b20140323.3", "2014", "The first version with the new settings menu", null, false, false, "https://cdn.titanic.sh/clients/b20140323.3.zip", new string[] { "d320f8e871ee5046688c9876996aa20d", "f6d0e1c16dc2b3f4490c2bd05e2b63a6", "460bdc443d98547ecbbdad6fb4183b2e", "b94a2053032e4bdd9278b3c4fa6e05e3", "97ada89560b89f704ff69f015ca37283" }, "/images/clients/small/b20140323.3.jpg", "2024-05-03T14:12:31.104537"),
                new ClientRelease("b20140127", "2014", "The version before the new settings menu was added", null, false, false, "https://cdn.titanic.sh/clients/b20140127.zip", new string[] { "5d6f3d09470cd54cc47e0bcf5e8e646a", "60e6fd692abac7e0968e53641a799375", "7fc235341c6ed770efbd0244fd8b119a", "6a2aa9ed814dfa66b3c771da2fb21837", "559f855015aa43c7c267ff1350830428" }, "/images/clients/small/b20140127.jpg", "2024-05-17T19:51:19.715805"),
                new ClientRelease("b20131129.1", "2013", "Contains a lot of improvements for tournaments", null, false, false, "https://cdn.titanic.sh/clients/b20131129.1.zip", new string[] { "d47ef68157ed98a4973a7b0d53eddc78", "7069a7c2eb0132d7c021cfd29dd15a47", "7a207923e3bf45c37cd58078017f49b9", "3cad3cf38facd4486606e7d9faf0cf06", "590fa052e810e185dc95454858e2f2b2", "aabc1594e19a5327e4a80a0eedcb994c" }, "/images/clients/small/b20131129.1.jpg", "2024-05-03T14:12:31.125568"),
                new ClientRelease("b20131113.2", "2013", "Yet another version from 2013", null, false, false, "https://cdn.titanic.sh/clients/b20131113.2.zip", new string[] { "1521c455a4e6d5cf9525c84743fbabd0", "6e2933d0e40014ae1d35367166268623" }, "/images/clients/small/b20131113.2.jpg", "2025-07-15T20:53:59.555823"),
                new ClientRelease("b20131024.1", "2013", "Added skin customisation of the menu glow color", null, false, false, "https://cdn.titanic.sh/clients/b20131024.1.zip", new string[] { "c5a0237e0ced21fd7aa7e9f5a02ebba3", "f9556e92d4be09680ee157d71c6bb2f7" }, "/images/clients/small/b20131024.1.jpg", "2025-07-15T20:51:42.950595"),
                new ClientRelease("b20131009", "2013", "This client adds localisations for Latvian and Slovene", null, false, false, "https://cdn.titanic.sh/clients/b20131009.zip", new string[] { "7cae49efab9618657096ca5ef318fd9c", "4de17687826525308e46524d632df231" }, "/images/clients/small/b20131009.jpg", "2025-07-15T20:49:19.296432"),
                new ClientRelease("b20130915.4", "2013", "Improved the song importing behavior on Linux/MacOS (wine)", null, false, false, "https://cdn.titanic.sh/clients/b20130915.4.zip", new string[] { "e7e63c58f15671e4cf947c2f9a3ae0b2", "d452864234b319188893a75ae68423dc" }, "/images/clients/small/b20130915.4.jpg", "2025-07-15T20:46:57.266195"),
                new ClientRelease("b20130823.3", "2013", "Changed the underlying protocol for bancho from tcp to http", null, false, false, "https://cdn.titanic.sh/clients/b20130823.3.zip", new string[] { "8a0d528ecc6c317f890924e413d00957", "7e54f25db59989338bd500387263af3b", "47201945e385327d5b04518a113e44f6" }, "/images/clients/small/b20130823.3.jpg", "2025-03-25T21:10:00.451627"),
                new ClientRelease("b20130802.1", "2013", "Removed the restriction for background dimming and other visual changes", null, false, false, "https://cdn.titanic.sh/clients/b20130802.1.zip", new string[] { "031713d5025403c1f7675a2603ce3149", "fdcd14e704098398cd59b29e4906f781" }, "/images/clients/small/b20130802.1.jpg", "2025-07-15T20:38:06.825925"),
                new ClientRelease("b20130716", "2013", "This client added some more features to collections", null, false, false, "https://cdn.titanic.sh/clients/b20130716.zip", new string[] { "8200c36a46359b1abe8db68a0a8d40ea", "87827dae2b3c93a76bd18165f1622527", "7e2dbc5dc7b649a1709be941caa8d181" }, "/images/clients/small/b20130716.jpg", "2025-03-25T21:08:30.954274"),
                new ClientRelease("b20130626", "2013", "Introduced full localisation of multiplayer", null, false, false, "https://cdn.titanic.sh/clients/b20130626.zip", new string[] { "857cfa177d75b35ab171e3633cb22e0c", "05e80e11cfe3493f7634ca99e0a80a57", "395cdf50e6356c510164f3b082583dbc" }, "/images/clients/small/b20130626.jpg", "2025-03-25T21:06:49.485728"),
                new ClientRelease("b20130606.1", "2013", "A build from 2013 with the new UI overhaul update", null, false, false, "https://cdn.titanic.sh/clients/b20130606.1.zip", new string[] { "5361cff8bad587ea8f54bd886b91bcc4", "8f14a97b6d11c1795fc42dcb784a3b7b", "5f26f2538731881cc70333495d00cef2", "2561cbb5dae2c450aae88b68a9a44ab3", "b4d96f63909061bfa4f541f9cf77529d", "dddffef60c07ec83843d769074212ad3", "8ec0f9b6481198580ec1f5a4b47ab571", "4efdadb56bcb210be8a133277e421751", "0a1d63d16b72f53920958e6ea6077b84" }, "/images/clients/small/b20130606.1.jpg", "2024-05-03T14:12:31.146575"),
                new ClientRelease("b20130603", "2013", "A mid\u20112013 build that removes the @osustatus in-game browser", null, false, false, "https://cdn.titanic.sh/clients/b20130603.zip", new string[] { "5ba40d7cdcfab111d59b9067f96778c8", "50520145f00fec7fbc5fa624fa576a2e" }, "/images/clients/small/b20130603.jpg", "2025-07-15T20:14:08.912798"),
                new ClientRelease("b20130513", "2013", "This client introduces difficulty names in osu! direct", null, false, false, "https://cdn.titanic.sh/clients/b20130513.zip", new string[] { "693f708adcc29a3778917fe496ae5cb2", "ac3bda64cb30ebdca0ef59fc2871e253", "95bb7168dce49f1018b1b170c63e0498" }, "/images/clients/small/b20130513.jpg", "2025-03-25T21:05:54.831907"),
                new ClientRelease("b20130329", "2013", "Added user avatars to song select leaderboards", null, false, false, "https://cdn.titanic.sh/clients/b20130329.zip", new string[] { "9be427a7dd68e7357b4b8264b8ea15c1", "6c5014539e25b3471154cd8a89cd5f29", "1ba8f2bb0dd85a4add0f5ea2bfc1cf0d" }, "/images/clients/small/b20130329.jpg", "2025-03-25T21:03:05.358280"),
                new ClientRelease("b20130319", "2013", "Released one day after the osu! UI revamp", null, false, false, "https://cdn.titanic.sh/clients/b20130319.zip", new string[] { "4ccd18c0170017e99da4582d1a18d237", "1c710e7ce28f4ff1a924e1f40b835ae2", "78b9a3843fc700b29c05bb8e7bd9a9c0" }, "/images/clients/small/b20130319.jpg", "2025-03-25T21:01:16.134847"),
                new ClientRelease("b20130303", "2013", "This is the last build before 2013's UI overhaul update.", null, false, true, "https://cdn.titanic.sh/clients/b20130303.zip", new string[] { "5055d087c2a7bb4a85459f9437a46307", "9f610d458cb69b9ea373d3dd6df1666c", "a068740bea8aac3db148e44f5f0754ae", "8ec0f9b6481198580ec1f5a4b47ab571", "b4fb4221684329e3778254bd3ddece21", "2b8af4273c61f8d0c1ec78e73de85cdb", "1502166bd25ab41637f68d532397a977", "aacf1ddcb6552f37b5f58cdd48f21d7a", "e22aaff067f038ef82c83e0cd37e2318", "817a8e9fa396f0ec408f1decb61403f0", "9c29d5c3ad9911b4d3d85539c3550cce", "a17b0db55739b81116f1f56374edc85a", "75ed7576ee316d5f0fc18d4743d89769", "3629b264ef876cc4ce29a4cf5ba87dda", "0f87916fc2a939fe504730ab52d2ba7d", "efe0b8ce1caabd4ce6e15b7a5d70d908", "8cee3e77cf86a3e0966abd54e4e3e293", "e6f23e4f0c34115bb686532755ed0c20", "4907ca58a8d50141d82f2e63d0431f21", "afa666a9b5a9801c3579d4935ea3326a", "986737fb3f82fed0e9479b889cca50df", "f76db48566db84c8af984909ada8deb6", "4724b20ca0f01a528aeb26b5985904fc", "8b335a8eee1f1fef9d8f67012dc9e0a3", "c97dd471c6ecb49d2ab27f7531375ad1", "b1ccbcc0d319deae2d6905401d24dfae" }, "/images/clients/small/b20130303.jpg", "2024-05-03T14:12:31.169059"),
                new ClientRelease("b20121223", "2012", "Made improvements to handling a large number of users in the bancho protocol", null, false, false, "https://cdn.titanic.sh/clients/b20121223.zip", new string[] { "b2696bc31ab065bd6385097c7af94fd5", "7c5652412fd8e809c93ebe12e42368b7", "6f95c76733e95cce3757ee263f643ccb", "665602e9b62ef6ec688bdeaed026289c", "8f064f77ebea7a761e7c581aba71ecb3" }, "/images/clients/small/b20121223.jpg", "2024-05-03T14:12:31.211150"),
                new ClientRelease("b20121203", "2012", "Added internal improvements to localisations", null, false, false, "https://cdn.titanic.sh/clients/b20121203.zip", new string[] { "193dc771794e8099ac86404b43005505", "d220f6ae93588949f4862a1c7751cd52", "8a69de411937270b5673e7ce93444a17" }, "/images/clients/small/b20121203.jpg", "2025-03-25T20:59:33.727641"),
                new ClientRelease("b20121119", "2012", "Added the ability to filter beatmaps by game mode", null, false, false, "https://cdn.titanic.sh/clients/b20121119.zip", new string[] { "bcae2db2df8be47f94d501d5d254938f", "056bc146701bf3119af88aea0338a0d6" }, "/images/clients/small/b20121119.jpg", "2025-03-25T20:58:36.152924"),
                new ClientRelease("b20121115", "2012", "Allows you to set a custom beatmap song path", null, false, false, "https://cdn.titanic.sh/clients/b20121115.zip", new string[] { "a344a34f30daf1191a8abd46cea9ed92", "b8dda326fc44ef3ffea4e9a9a051e04e", "54cab457793e45ec09cdf84500410f67" }, "/images/clients/small/b20121115.jpg", "2025-03-25T20:57:24.547192"),
                new ClientRelease("b20121023", "2012", "Made lots of improvements to osu! mania after its initial release", null, false, false, "https://cdn.titanic.sh/clients/b20121023.zip", new string[] { "c00d001ae6af55d031d6d490cbd750de", "63c7d47db0df7cdf3e5aaf3dcb387e48", "a6e80f6f5f75dc24834b47a9036d7395" }, "/images/clients/small/b20121023.jpg", "2025-03-25T20:55:58.412157"),
                new ClientRelease("b20121003shine.test", "2012", "The first version to add osu!mania", null, false, false, "https://cdn.titanic.sh/clients/b20121003shine.test.zip", new string[] { "8c015e5fed1361b1ac8bd4b06d08f140", "c2883d682cf32d0db1f1acf5485ffcfb", "41fe234853ff9e1282f57b068117dfa1", "526ad5905d182a7a499763c62e2d112e", "b4a3404f0c6ad608cf1217d9bd70fd13" }, "/images/clients/small/b20121003shine.test.jpg", "2024-05-03T14:12:31.233390"),
                new ClientRelease("b20120916", "2012", "Adds skinning support for multiplayer & osu! direct backgrounds", null, false, true, "https://cdn.titanic.sh/clients/b20120916.zip", new string[] { "563763d859e0f227affacecd043f77a1", "21adeccc8a379b49a5dc7cec815552ec", "56aec6cacee98bfb79bd64d41fb697d6", "133b621671c69dfe3c2ace4ff908902f", "b83defa051c092f1b919d399d3ab85d3" }, "/images/clients/small/b20120916.jpg", "2024-05-03T14:12:31.253304"),
                new ClientRelease("b20120812", "2012", "Added the \"Perfect\" mod", null, false, false, "https://cdn.titanic.sh/clients/b20120812.zip", new string[] { "bbc109ccbcf0fbfb916858a1e29c0e02", "b9705a41513a5ada5e0077439552e963", "f8e0f189eac265414c97ce5280bd64ae" }, "/images/clients/small/b20120812.jpg", "2025-03-25T20:52:32.185356"),
                new ClientRelease("b20120529", "2012", "Fixed RX/AP being selectable with NF", null, false, false, "https://cdn.titanic.sh/clients/b20120529.zip", new string[] { "7110ea84237601dab2ba7c73a6fd89a3", "d278dd1250e5b6048f0ff62f751ac54c", "a3b7270806df530f565a9575e24bb997" }, "/images/clients/small/b20120529.jpg", "2024-12-29T20:19:52.440239"),
                new ClientRelease("b20120522", "2012", "Now warns you when trying to DM an offline user", null, false, false, "https://cdn.titanic.sh/clients/b20120522.zip", new string[] { "170cb44805adc55b65160908e28642fc", "0f4ccbd268b90ca29631533a16a98f77", "6ff88ed9281528ad20eb58da34e1b8cb", "0c43143d964fd2ecf5c50417fbeba05f", "2bc81888d1515da669ebb0e4f67f42f3", "27a9f0b421d851409504b2174059de64", "9dddecfbae724d6be2347630a199b12c" }, "/images/clients/small/b20120522.jpg", "2024-05-03T14:12:31.274305"),
                new ClientRelease("b1844.test", "2011", "This build was used to test the \"osz2\" file format", null, false, false, "https://cdn.titanic.sh/clients/b1844.test.zip", new string[] { "7033eb078c55d22606a4562973d0068c", "2fe2002c28bb08993a27a51a7fe5fcd7", "98a5f2ea187225ae18ef6e1675bdded1", "7c656b6e0db1dd4dcd0fa0be87a60987", "383faea0f2b74e55717008ff96e1de9c" }, "/images/clients/small/b1844.test.jpg", "2024-05-03T14:12:31.295365"),
                new ClientRelease("b1821", "2012", "", null, false, false, "https://cdn.titanic.sh/clients/b1821.zip", new string[] { "0e9558ad9eb4fda87aa147bb920f5fe7", "97520f952bc80c9372ce73a23cd1a0fb", "3be49c1ff0247c98ec7780b2485e296f", "38cf885bfc2babe76c296351b46ebdfd", "d4a15c6323adf0bbfaff4aaf0ae1c46b" }, "/images/clients/small/b1821.jpg", "2024-05-03T14:12:31.316389"),
                new ClientRelease("b1818", "2011", "This client changed update checks to be more efficient with less bandwidth usage", null, false, true, "https://cdn.titanic.sh/clients/b1818.zip", new string[] { "230682d73d93769494d6b4c42e6e586b", "af24504fa8ccf210f3b789a61708fc53" }, "/images/clients/small/b1818.jpg", "2024-12-29T20:16:53.729940"),
                new ClientRelease("b1815", "2011", "Updated the client to change server file locations for static content", null, false, false, "https://cdn.titanic.sh/clients/b1815.zip", new string[] { "44e088f5b8f343d6d9ecfe66c1bf5edf", "402ba4cfcfe5bfd5408f8a8648594908", "444e23203ffde9b28fee4ad900ba30c6", "040e931ee31d22b1af44f1849a6862c5" }, "/images/clients/small/b1815.jpg", "2024-05-17T19:41:20.852737"),
                new ClientRelease("b1814", "2011", "", null, false, false, "https://cdn.titanic.sh/clients/b1814.zip", new string[] { "565da88e6cefecb2824b755b9e30f742", "5a7e6aa8b5a1fd3688f97cb7eb5d0e6e" }, "/images/clients/small/b1814.jpg", "2024-12-29T20:14:01.344162"),
                new ClientRelease("b1807", "2011", "Released on april fools, forcing you to play with nightcore", null, false, false, "https://cdn.titanic.sh/clients/b1807.zip", new string[] { "3ee965ff3ab6b9593818efcbcb6666d2", "0986fbc5cd720af71a655f42a6b5e290", "4be1865a8bba9c7c9707848c2a7c2ed5" }, "/images/clients/small/b1807.jpg", "2025-03-31T22:15:09.036213"),
                new ClientRelease("b1704", "2010", "Same as b1700, correcting the build number to b1704", null, false, true, "https://cdn.titanic.sh/clients/b1704.zip", new string[] { "eec9475d17300cebfdce046fb21ad884", "2bb1f04ca3e033cb3afeed5c0136e76d" }, "/images/clients/small/b1704.jpg", "2024-12-29T20:06:29.443326"),
                new ClientRelease("b1700", "2010", "", null, false, false, "https://cdn.titanic.sh/clients/b1700.zip", new string[] { "f1afa096f831fa3499571b6b65ceb737", "e61522a3e4a9a1ec18f2e066180dd5d1", "94456a08f6f34127973002c98916847c", "2ddd74144fbea114495d4e28917db25e", "99e276fe6b170fa64c84b9655685c902" }, "/images/clients/small/b1700.jpg", "2024-05-03T14:12:31.337474"),
                new ClientRelease("b1652", "2010", "This is the first build to have split AR and OD", null, false, false, "https://cdn.titanic.sh/clients/b1652.zip", new string[] { "41f86469548d9b2b654a523b96096dc2", "c83f014a7c9a29215da7a1de8056df15" }, "/images/clients/small/b1652.jpg", "2024-12-29T20:04:45.016001"),
                new ClientRelease("b1596", "2010", "Added AiMod improvements, including an experimental difficulty star system", null, false, false, "https://cdn.titanic.sh/clients/b1596.zip", new string[] { "d4ffc6a66eb7b46687efac49f2464cbb", "95cf30e06e332516f11f1a46d73f3b41", "af9265d333410bbbad55c453bccb1bcf" }, "/images/clients/small/b1596.jpg", "2024-05-03T14:12:31.358511"),
                new ClientRelease("b1553", "2010", "", null, false, false, "https://cdn.titanic.sh/clients/b1553.zip", new string[] { "431e5ed0a09bab73f431776296c8252f", "c62d7da7ff041c722b54ce47e11d8147" }, "/images/clients/small/b1553.jpg", "2024-12-29T19:58:20.423186"),
                new ClientRelease("b1218", "2010", "", null, false, false, "https://cdn.titanic.sh/clients/b1218.zip", new string[] { "be4a5fbfbca3e46ec0c957daf9100116", "210928a5741d7f0777304e9480a0ccad", "4e696468cbe7a9f5424b38e26b588c6c" }, "/images/clients/small/b1218.jpg", "2024-05-03T14:12:31.379567"),
                new ClientRelease("b1122", "2009", "", null, false, false, "https://cdn.titanic.sh/clients/b1122.zip", new string[] { "41a64e750847105e406e0093c7bba5fc", "064a1437613a5f545d7d1837979119a2", "ba5758b863cbd9c66a95024decce324f" }, "/images/clients/small/b1122.jpg", "2024-05-03T14:12:31.400601"),
                new ClientRelease("b1077a", "2009", "This is the first version of osu! with proper widescreen support", null, false, true, "https://cdn.titanic.sh/clients/b1077a.zip", new string[] { "9ceb72400e52f8c117269cc29ea914cc", "0971459e7bb3c9391602325f05a03121", "833f59f6cc3e180a2007199065e61577" }, "/images/clients/small/b1077a.jpg", "2024-05-03T14:12:31.421591"),
                new ClientRelease("b904", "2009", "A build from 2009 that is used in the oldsu server", null, false, false, "https://cdn.titanic.sh/clients/b904.zip", new string[] { "2e7ad1a6986cbcdb9b676a9fb5ea090c", "7123631119a5be05be03be7b166b7372", "791ece08ba9dea2ef3ac45da654ee9f7", "a2c8319fff784691435e0f7265413b8e" }, "/images/clients/small/b904.jpg", "2024-05-03T14:12:31.442695"),
                new ClientRelease("b833", "2009", "This version has a very early and buggy implementation of widescreen", null, false, false, "https://cdn.titanic.sh/clients/b833.zip", new string[] { "e3c077f35bf88fc4a0025508436964b1", "5e0c2749d4d810baf32022c1dc0156b6" }, "/images/clients/small/b833.jpg", "2024-12-29T19:55:47.194643"),
                new ClientRelease("b753c", "2009", "A build from 2009 that added local rankings", null, false, false, "https://cdn.titanic.sh/clients/b753c.zip", new string[] { "622f9cc5ecdb072ad1d2bfad939fd4ee", "013a43809a580c63703c3e9238746449", "8745c3dd69182b916ca49cc6d124737f" }, "/images/clients/small/b753c.jpg", "2024-05-03T14:12:31.463884"),
                new ClientRelease("b699", "2009", "nice?", null, false, false, "https://cdn.titanic.sh/clients/b699.zip", new string[] { "98c2a99fdc1e93f22b1b351f74d6d70d", "2e4469519881be3d41b7b2f8649360d2", "78479c0e1c33d48b9dad6de40a2aa6ad" }, "/images/clients/small/b699.jpg", "2024-05-03T14:12:31.484912"),
                new ClientRelease("b639", "2009", "The first version to introduce the 2009-2011 menu, requested by s3tup", null, false, false, "https://cdn.titanic.sh/clients/b639.zip", new string[] { "b23ee57bbeb207077c0180027418e3ed", "646d48cf51148fac4151a81f95c2e289" }, "/images/clients/small/b639.jpg", "2025-07-22T14:13:03.539389"),
                new ClientRelease("b595b", "2009", "Improved stability of the editor, backgrounds & multiplayer matches", null, false, false, "https://cdn.titanic.sh/clients/b595b.zip", new string[] { "118829b412d8651dfd7b903f5701f715", "0df57295b633d5bf16f4cd1b6d463c44" }, "/images/clients/small/b595b.jpg", "2024-12-29T19:53:32.708175"),
                new ClientRelease("b558", "2008", "", null, false, false, "https://cdn.titanic.sh/clients/b558.zip", new string[] { "74d41b6ab5dd31d0477c88ca1cf32de0" }, "/images/clients/small/b558.jpg", "2025-04-06T16:25:48.612022"),
                new ClientRelease("b534.test", "2008", "This version was used to test the MSN integration", null, false, false, "https://cdn.titanic.sh/clients/b534.test.zip", new string[] { "2aaed82fff9b778c8276ff045e864fd7" }, "/images/clients/small/b534.test.jpg", "2025-04-06T16:22:34.427238"),
                new ClientRelease("b504", "2008", "The first version of osu! with catch the beat support", null, false, false, "https://cdn.titanic.sh/clients/b504.zip", new string[] { "b940e620c556031d2d70fa3611a1ea05", "778e41018c3f23f062a51cce53150ebd", "4dba41e24d9bba92f4e57d7d7653f5a7" }, "/images/clients/small/b504.jpg", "2024-05-03T14:12:31.506033"),
                new ClientRelease("b497", "2008", "This version of osu! adds a new song select style and the first version of osu!direct", null, false, true, "https://cdn.titanic.sh/clients/b497.zip", new string[] { "9d38cac4bca771199b0a2a98f8dfdf46", "e94d1f53ee1a06391d2fa44aec1da115", "a20d0d72a6e4af5cde416636bc0a5a81" }, "/images/clients/small/b497.jpg", "2024-05-03T14:12:31.527009"),
                new ClientRelease("b476", "2008", "A build from 2008 with very early support for Catch the Beat", null, false, false, "https://cdn.titanic.sh/clients/b476.zip", new string[] { "9367780374a46bb11389a8c4e6629fe7", "2a54efb71b1c39be7cc1566d0e9736e7" }, "/images/clients/small/b476.jpg", "2024-12-29T19:50:14.830355"),
                new ClientRelease("b452", "2008", "A very early OpenGL build, that also features work-in-progress sliders", null, false, false, "https://cdn.titanic.sh/clients/b452.zip", new string[] { "ea40fe542329951e891e8846de0c97af", "25456462bc2c1b32163d85c7598dfd3f", "4a6144dd47928c9bea3c314f48c4125f" }, "/images/clients/small/b452.jpg", "2024-05-03T14:12:31.548047"),
                new ClientRelease("b420", "2008", "A build from 2008, the first public build featuring the Auto and SpunOut mods", "Please note that all versions before b452.1 can't load newer beatmaps properly!", false, false, "https://cdn.titanic.sh/clients/b420.zip", new string[] { "2c3df52a044f4965d7d52a459eb806e4", "e22f8f7a5d5293e5ca28e412733a4c71", "11f40ed3db477afecc7e20d835809f4e" }, "/images/clients/small/b420.jpg", "2024-05-03T14:12:31.569359"),
                new ClientRelease("b394a", "2008", "A build from 2008 that oldsu used in the past", null, false, false, "https://cdn.titanic.sh/clients/b394a.zip", new string[] { "03060f9f96a391c7baeb3dd6a5f45158", "aafc662f12e87295652e43f9dfc95b8a" }, "/images/clients/small/b394a.jpg", "2024-05-14T10:03:44.998941"),
                new ClientRelease("b370", "2008", "A shady version of osu! found by tokaku on shopee. Finally decompressed and patched by Oreeeee!", null, false, false, "https://cdn.titanic.sh/clients/b370.zip", new string[] { "b350511e572b2314db7f47ab2699ecf8", "61bb230b825a4fc7bd18fecaabbd1cc8" }, "/images/clients/small/b370.jpg", "2025-03-29T16:20:08.903245"),
                new ClientRelease("b337", "2008", "The earliest version with multiplayer support that was archived", null, false, false, "https://cdn.titanic.sh/clients/b337.zip", new string[] { "31eca86a78bd0984edb70881723266a3", "257df0ea394bd2d797f7d1e327e887cd" }, "/images/clients/small/b337.jpg", "2024-05-17T19:33:47.394734"),
            };
        }
    }
}
