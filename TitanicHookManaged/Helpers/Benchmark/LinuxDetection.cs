using System;
using System.IO;

namespace TitanicHookManaged.Helpers.Benchmark;

public class LinuxDetection
{
    public static string GetLinuxDistroInfo()
    {
        try
        {
            // Attempt to read from /etc/os-release through the Wine Z: drive
            string osReleasePath = @"Z:\etc\os-release";
            if (File.Exists(osReleasePath))
            {
                // Read and parse the file to find the "PRETTY_NAME" entry
                foreach (string line in File.ReadAllLines(osReleasePath))
                {
                    if (line.StartsWith("PRETTY_NAME"))
                    {
                        string distroName = line.Split('=')[1].Trim('"');

                        // If the distro name contains 'flatpak', try checking /run/host/os-release
                        if (distroName.ToLower().Contains("flatpak"))
                        {
                            string hostOsReleasePath = @"Z:\run\host\os-release";
                            if (File.Exists(hostOsReleasePath))
                            {
                                foreach (string hostLine in File.ReadAllLines(hostOsReleasePath))
                                {
                                    if (hostLine.StartsWith("PRETTY_NAME"))
                                    {
                                        // Extract and return the host's real distro name
                                        return hostLine.Split('=')[1].Trim('"');
                                    }
                                }
                            }

                            // If no host OS is found, return the flatpak name
                            return distroName;
                        }

                        return distroName;
                    }
                }
            }

            // Fallback to /proc/version if /etc/os-release is not found
            string procVersionPath = @"Z:\proc\version";
            if (File.Exists(procVersionPath))
            {
                return File.ReadAllText(procVersionPath).Trim();
            }

            // If both files are not accessible, return a generic message
            return "Unknown Linux Distro";
        }
        catch (Exception ex)
        {
            // If there was an error reading the files, log or return the error message
            return "Error fetching Linux Distro: " + ex.Message;
        }
    }
}
