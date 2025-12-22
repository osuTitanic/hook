using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using TitanicHookManaged;
using TitanicHookManaged.Helpers;

namespace HookLoader;

public static class Updater
{
    private static readonly string SelfFilename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
    
    public static void CheckForUpdates()
    {
        string variant = "";
#if NET20
        variant = "net20";
#elif NET40
        variant = "net40";
#endif
        
        string selfChecksum = ChecksumUtils.CalculateSha256(SelfFilename);

        byte[] data;
        using (var wc = new WebClient())
        {
            string url = $"{Constants.UpdateServer}/update?type={variant}&checksum={selfChecksum}";
            try
            {
                data = wc.DownloadData(url);
            }
            catch (Exception e)
            {
                Logging.Info($"Failed to download update: {e}");
                return;
            }
        }
        bool updateFound = data.Length > 0;
        if (updateFound)
        {
            var result = MessageBox.Show("A new version of Titanic! is available! Update now?", "Titanic! Updater", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.No) return;
        }
        else
        {
            Logging.Info("Up to date!");
            return;
        }
            
        // Update got downloaded, self-replace the file
        File.Move(SelfFilename, $"{SelfFilename}.old");
        File.WriteAllBytes($"{SelfFilename}", data);
        Process.Start(SelfFilename);
        Environment.Exit(0);
    }
    
    public static bool DeleteTempFile()
    {
        bool oldFileFound = File.Exists($"{SelfFilename}.old");
        if (oldFileFound)
            File.Delete($"{SelfFilename}.old");
        return oldFileFound;
    }
}
