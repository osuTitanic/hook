using TitanicHook.Core.Helpers;
using System.Reflection;
using System.IO;

namespace TitanicHook.Standalone
{
    internal class Program
    {
        /// <summary>
        /// Original entry assembly (before hooking)
        /// </summary>
        private static Assembly? _originalEntryAssembly;

        static void Main(string[] args)
        {
#if !DEBUG
            // Update check
            Loader.OsuLoader.AutoUpdated = Updater.DeleteTempFile();
            Updater.CheckForUpdates();
#endif

            _originalEntryAssembly = Assembly.GetEntryAssembly();
            if (_originalEntryAssembly == null)
            {
                Logging.LogAndShowError("Assembly.GetEntryAssembly() returned null");
                return;
            }

            string? path = OsuFileUtils.FindOsuExecutable(Path.GetDirectoryName(_originalEntryAssembly.Location));
            Loader.OsuLoader.Start(path!);
        }
    }
}
