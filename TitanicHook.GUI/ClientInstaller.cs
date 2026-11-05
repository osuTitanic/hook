using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using TitanicHook.Core.Helpers;

namespace TitanicHook.GUI
{
    internal static class ClientInstaller
    {
        private const long MaxDownloadBytes = 1024L * 1024 * 1024; // around 1 gig
        private const string StagingDirectory = ".titanic";
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();

        private static readonly string[] ProtectedFiles = {
            // dependencies from debug builds
            "Newtonsoft.Json.dll", "ICSharpCode.SharpZipLib.dll", "TitanicHook.Core.dll", "LinqBridge.dll",
            "TitanicHook.Loader.dll", "TitanicHook.PluginBase.dll", "0Harmony.dll", "ILReader.dll",
        };

        public static void Install(string target, Release release, Action<string, int?> progress)
        {
            // download url may start with / so we need to prepend the base url in that case
            Uri url = new Uri(new Uri("https://osu.titanic.sh/"), release.DownloadUrl!);

            using (LockInstallation(target))
            {
                string stagingPath = Path.Combine(target, StagingDirectory);

                if (Directory.Exists(stagingPath))
                    // get out of the way! we want to install something
                    RemoveSafe(stagingPath);

                Directory.CreateDirectory(stagingPath);
                try
                {
                    string archivePath = Path.Combine(stagingPath, "release.zip");
                    string extractedPath = Path.Combine(stagingPath, "release");

                    Download(url, archivePath, progress);
                    progress("Extracting client...", null);
                    Extract(archivePath, extractedPath);

                    string executable = OsuFileUtils.FindOsuExecutable(extractedPath)
                        ?? throw new InvalidDataException("The release must contain an osu! executable");

                    string executableChecksum = ChecksumUtils.CalculateMd5(executable);

                    if (!release.Matches(executableChecksum))
                        throw new InvalidDataException("The release executable does not match the expected checksum");

                    progress("Switching client...", null);
                    ReplaceFiles(target, extractedPath, stagingPath);
                    RemoveSafe(stagingPath);
                }
                catch
                {
                    RestoreBackup(target, stagingPath);
                    throw;
                }
            }
        }
        public static void TryRecover(string targetPath)
        {
            string stagingPath = Path.Combine(targetPath, StagingDirectory);

            if (!Directory.Exists(stagingPath))
                return;

            using (LockInstallation(targetPath))
            {
                if (Directory.Exists(stagingPath))
                    RestoreBackup(targetPath, stagingPath);
            }
        }

        private static void Download(Uri url, string archive, Action<string, int?> progress)
        {
            progress("Downloading client...", 0);

            using (WebResponse response = WebRequest.Create(url).GetResponse())
            using (Stream input = response.GetResponseStream())
            using (FileStream output = File.Create(archive))
            {
                long size = response.ContentLength;
                if (size > MaxDownloadBytes)
                    throw new InvalidDataException("The release archive is too large");

                int lastPercentage = 0;
                if (size <= 0)
                    // can't display a progress bar without the content length
                    progress("Downloading client...", null);

                byte[] buffer = new byte[81920]; // 80 kib
                long received = 0;
                int count;

                while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    received += count;
                    if (received > MaxDownloadBytes)
                        throw new InvalidDataException("The release archive is too large");

                    output.Write(buffer, 0, count);
                    if (size <= 0)
                        continue;

                    int percentage = (int)Math.Min(100, received * 100 / size);
                    if (percentage == lastPercentage)
                        continue;
                    
                    progress("Downloading client...", percentage);
                    lastPercentage = percentage;
                }
            }
        }
        private static void Extract(string archive, string destination)
        {
            Directory.CreateDirectory(destination);

            using (var zip = new ZipInputStream(File.OpenRead(archive)))
            {
                ZipEntry entry;

                while ((entry = zip.GetNextEntry()) != null)
                {
                    if (entry.IsDirectory)
                        continue;

                    string relative = entry.Name;

                    // remove leading ./ from the path to avoid creating a directory named "."
                    while (relative.StartsWith("./", StringComparison.Ordinal))
                        relative = relative.Substring(2);

                    // windows uses backslashes for paths, linux/mac do not
                    relative = relative.Replace('/', Path.DirectorySeparatorChar);

                    string path = ResolvePathSafe(destination, relative);
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    using (var output = new FileStream(path, FileMode.CreateNew))
                        CopyTo(zip, output);
                }
            }
        }

        private static void ReplaceFiles(string targetPath, string releasePath, string stagingPath)
        {
            List<string> newFiles = new List<string>();
            Dictionary<string, bool> affected = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            // find all relevant files in the release folder
            foreach (string path in Directory.GetFiles(releasePath))
            {
                string file = Path.GetFileName(path);

                if (!IsOsuFile(file))
                    continue;

                newFiles.Add(file);
                affected[file] = true;
            }

            // find all relevant files in the target folder
            foreach (string path in OsuFileUtils.FindOsuExecutables(targetPath))
            {
                string file = Path.GetFileName(path);

                if (IsOsuFile(file))
                    affected[file] = true;
            }

            // create a backup of the affected files in case something goes wrong
            string backup = Path.Combine(stagingPath, "backup");

            foreach (string file in affected.Keys)
            {
                string path = ResolvePathSafe(targetPath, file);

                if (!File.Exists(path))
                    continue;

                // ensure the file is not in use before moving it to the backup folder
                using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None)) { }

                MoveFileSafe(path, Path.Combine(backup, file));
            }

            // move the new files into place
            foreach (string file in newFiles)
                MoveFileSafe(Path.Combine(releasePath, file), Path.Combine(targetPath, file));
        }

        private static bool IsOsuFile(string file)
        {
            if (file != Path.GetFileName(file))
                return false;

            string extension = Path.GetExtension(file);

            // we only care about .dll and .exe files
            if (!string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
                return false;

            string launcherPath = Path.GetFileName(Application.ExecutablePath);

            if (Array.Exists(ProtectedFiles, name => string.Equals(file, name, StringComparison.OrdinalIgnoreCase)))
                return false;

            if (string.Equals(file, launcherPath, StringComparison.OrdinalIgnoreCase))
                return false;

            if (file.StartsWith(launcherPath + ".", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        private static FileStream LockInstallation(string targetPath)
        {
            return new FileStream(
                Path.Combine(targetPath, ".titanic.lock"),
                FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose
            );
        }

        // doesn't exist in .net framework 2 so here's our own scuffed implementation
        private static void CopyTo(Stream source, Stream destination)
        {
            byte[] buffer = new byte[81920];

            int count;
            while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
                destination.Write(buffer, 0, count);
        }

        private static void RestoreBackup(string targetPath, string stagingPath)
        {
            string backup = Path.Combine(stagingPath, "backup");

            if (Directory.Exists(backup))
            {
                // restore from backup folder
                foreach (string path in Directory.GetFiles(backup))
                    MoveFileSafe(path, ResolvePathSafe(targetPath, Path.GetFileName(path)));
            }

            RemoveSafe(stagingPath);
        }

        private static void MoveFileSafe(string source, string destination)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Move(source, destination);
        }

        private static void RemoveSafe(string stagingPath)
        {
            try
            {
                Directory.Delete(stagingPath, true);
            }
            catch (IOException ex)
            {
                Logging.Error("Failed to clean client switch files: " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logging.Error("Failed to clean client switch files: " + ex.Message);
            }
        }

        private static string ResolvePathSafe(string root, string relative)
        {
            if (string.IsNullOrEmpty(relative) || Path.IsPathRooted(relative))
                throw new InvalidDataException("Unsafe client file path: " + relative);

            if (relative.IndexOfAny([':', '/']) >= 0)
                throw new InvalidDataException("Unsafe client file path: " + relative);

            foreach (string part in relative.Split(Path.DirectorySeparatorChar))
            {
                if (part.Length == 0 || part.EndsWith(".") || part.EndsWith(" "))
                    throw new InvalidDataException("Unsafe client file path: " + relative);

                if (part.IndexOfAny(InvalidFileNameChars) >= 0)
                    throw new InvalidDataException("Unsafe client file path: " + relative);

                if (File.Exists(root))
                    throw new IOException("A file uses a parent directory of: " + relative);

                root = Path.Combine(root, part);

                if ((File.Exists(root) || Directory.Exists(root)) &&
                    (File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("Client file path contains a link: " + relative);
            }

            return root;
        }
    }
}
