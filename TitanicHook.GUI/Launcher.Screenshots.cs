using System;
using System.Drawing;
using System.IO;
using System.Net;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    public partial class Launcher
    {
        private WebClient? screenshotRequest;

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

    }
}
