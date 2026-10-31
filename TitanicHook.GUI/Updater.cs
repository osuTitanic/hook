using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TitanicHook.Core;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    internal sealed class Updater : IDisposable
    {
        private WebClient? request;
        private Action<int?>? onProgress;
        private Action<byte[]?, Exception?>? onCompleted;

        public void Check(bool useFallback, Action<int?> progress, Action<byte[]?, Exception?> completed)
        {
            onProgress = progress;
            onCompleted = completed;

            try
            {
                string checksum = ChecksumUtils.CalculateSha256(Application.ExecutablePath);
                string variant = useFallback ? "net20" : "net40";
                string url = Constants.UpdateServer + "/update?type=" + variant + "&checksum=" + checksum;
                
                request = new WebClient();
                request.DownloadProgressChanged += OnProgressChanged;
                request.DownloadDataCompleted += OnDownloaded;
                request.DownloadDataAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                Dispose();
                completed(null, ex);
            }
        }

        private void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!object.ReferenceEquals(sender, request))
                return;

            if (e.TotalBytesToReceive <= 0)
                return;

            int progress = Math.Max(0, Math.Min(100, e.ProgressPercentage));
            onProgress?.Invoke(progress);
        }

        private void OnDownloaded(object sender, DownloadDataCompletedEventArgs e)
        {
            if (!object.ReferenceEquals(sender, request))
                return;

            Action<byte[]?, Exception?>? completed = onCompleted;
            Dispose();

            if (e.Cancelled)
                return;
            
            completed?.Invoke(e.Error == null ? e.Result : null, e.Error);
        }

        public static void Install(byte[] update, bool restart = true)
        {
            string executable = Application.ExecutablePath;
            string oldFile = executable + ".old";
            string newFile = executable + ".new";
            bool movedOriginal = false;

            try
            {
                File.WriteAllBytes(newFile, update);

                if (File.Exists(oldFile))
                    File.Delete(oldFile);
                
                File.Move(executable, oldFile); // Titanic.exe -> Titanic.exe.old
                movedOriginal = true;
                File.Move(newFile, executable); // Titanic.exe.new -> Titanic.exe
                
                if (restart) Process.Start(executable);
            }
            catch
            {
                if (movedOriginal)
                {
                    if (File.Exists(executable))
                        File.Delete(executable);
                    
                    File.Move(oldFile, executable); // Titanic.exe.old -> Titanic.exe
                }
                throw;
            }
            finally
            {
                try { if (File.Exists(newFile)) File.Delete(newFile); }
                catch { }
            }
        }

        public void Dispose()
        {
            if (request != null)
            {
                request.CancelAsync();
                request.Dispose();
                request = null;
            }
            onProgress = null;
            onCompleted = null;
        }
    }
}
