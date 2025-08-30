using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
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
        if (data.Length == 0)
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
    
    public static void DeleteTempFile()
    {
        if (File.Exists($"{SelfFilename}.old"))
            File.Delete($"{SelfFilename}.old");
    }
}
