using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using TitanicHook.Core;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    internal sealed class Updater : IDisposable
    {
        private readonly string executable = Application.ExecutablePath;

        private WebClient? request;
        private Action<int?>? onProgress;
        private Action<byte[]?, Exception?>? onCompleted;

        public void Check(bool useFallback, Action<int?> progress, Action<byte[]?, Exception?> completed)
        {
            onProgress = progress;
            onCompleted = completed;

            try
            {
                string checksum = ChecksumUtils.CalculateSha256(executable);
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

        public static void CheckUnattended()
        {
            Updater updater = new Updater();
            ThreadPool.QueueUserWorkItem(
                _ => updater.Check(Environment.Version.Major < 4, progress => { }, updater.OnBackgroundUpdateCompleted));
        }

        private void OnBackgroundUpdateCompleted(byte[]? update, Exception? error)
        {
            if (error != null)
            {
                Logging.Error("Failed to check for updates: " + error);
                return;
            }

            if (update == null || update.Length <= 0)
                return;

            try
            {
                Install(update, restart: false);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to install update: " + ex);
            }
        }

        public void Install(byte[] update, bool restart = true)
        {
            string suffix = Guid.NewGuid().ToString("N");
            string oldFile = executable + ".old." + suffix;
            string newFile = executable + ".new." + suffix;
            bool movedOriginal = false;

            try
            {
                File.WriteAllBytes(newFile, update);

                File.Move(executable, oldFile); // Titanic.exe -> Titanic.exe.old.<guid>
                movedOriginal = true;
                File.Move(newFile, executable); // Titanic.exe.new.<guid> -> Titanic.exe
                
                if (restart) Process.Start(executable);
            }
            catch
            {
                if (movedOriginal)
                {
                    if (File.Exists(executable))
                        File.Delete(executable);
                    
                    File.Move(oldFile, executable); // Titanic.exe.old.<guid> -> Titanic.exe
                }
                throw;
            }
            finally
            {
                try { if (File.Exists(newFile)) File.Delete(newFile); }
                catch { }
            }

            // Hide our backup executable
            try { File.SetAttributes(oldFile, File.GetAttributes(oldFile) | FileAttributes.Hidden); }
            catch { }
        }

        public static void CleanupOldVersions()
        {
            string executable = Application.ExecutablePath;
            string directory = Path.GetDirectoryName(executable)!;
            string prefix = Path.GetFileName(executable) + ".old.";

            try
            {
                // Remove regular .old files (may be left from a previous version)
                try { File.Delete(executable + ".old"); }
                catch { }

                foreach (string file in Directory.GetFiles(directory, prefix + "*"))
                {
                    try { File.Delete(file); }
                    catch { }
                }
            }
            catch { }
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
