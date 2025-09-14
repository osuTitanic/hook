using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;
using Microsoft.Win32;
using TitanicHookManaged.OsuInterop;

namespace TitanicHookManaged.Helpers.Benchmark;

public static class HardwareDetection
{
    private static string HKLM_GetString(string path, string key)
    {
	    try
	    {
		    RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
		    if (rk == null) return "";
		    return (string)rk.GetValue(key);
	    }
	    catch { return ""; }
    }

    public static Hardware GetHardwareInfo(bool submitFullHardware)
    {
        var hw = new Hardware();
        
        ConfigReader osuCfg = new ();
        string rendererInput = osuCfg.TryGetValue("Renderer");

        if (rendererInput == "opengl")
            hw.renderer = "OpenGL";
        else if (rendererInput == "d3d")
            hw.renderer = "DirectX";
        else
        {
            MessageBox.Show("The renderer did not return data correctly!");
            hw.renderer = "Unknown Renderer";
        }

        if (!submitFullHardware)
            return hw;

        ManagementObjectSearcher searcher;

        bool cpuInfoRetrieved = false;

        // Try to query CPU information with WMI
        try
        {
            searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                hw.cpu = obj["Name"]?.ToString() ?? "Unknown CPU";

                // Use `NumberOfCores` but fallback to `NumberOfLogicalProcessors` if unavailable
                hw.cores = obj["NumberOfCores"] != null ? Convert.ToInt32(obj["NumberOfCores"]) :
                        (obj["NumberOfLogicalProcessors"] != null ? Convert.ToInt32(obj["NumberOfLogicalProcessors"]) / 2 : 0); // Guess 2 cores for older processors

                // Use `ThreadCount` if available, otherwise fallback to `NumberOfLogicalProcessors`
                hw.threads = obj["ThreadCount"] != null ? Convert.ToInt32(obj["ThreadCount"]) :
                        (obj["NumberOfLogicalProcessors"] != null ? Convert.ToInt32(obj["NumberOfLogicalProcessors"]) : 0);

                if (hw.cpu != "Unknown CPU" && hw.cores > 0 && hw.threads > 0)
                {
                    cpuInfoRetrieved = true;
                    break;
                }
            }
        }
        catch
        {
            cpuInfoRetrieved = false;
        }

        // If WMI CPU information failed or was incomplete, fallback to using the HardwareFinder class
        if (!cpuInfoRetrieved)
        {
            try
            {
                List<CPU> cpus = HardwareFinder.GetCPUs();
                if (cpus.Count > 0)
                {
                    CPU firstCPU = cpus[0];  // Use the first CPU in the list
                    hw.cpu = firstCPU.Name;
                    hw.cores = (int)firstCPU.Cores;
                    hw.threads = (int)firstCPU.LogicalCpus;

                    if (hw.cpu != "Unknown CPU" && hw.cores > 0 && hw.threads > 0)
                        cpuInfoRetrieved = true;
                }
            }
            catch (Exception)
            {
            }
        }

        // If both WMI and HardwareFinder failed, fallback to using the Windows Registry
        if (!cpuInfoRetrieved)
        {
            try
            {
                string registryKey = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        hw.cpu = key.GetValue("ProcessorNameString")?.ToString() ?? "Unknown CPU";
                        hw.motherboard = key.GetValue("Identifier")?.ToString() ?? "Unknown Identifier";
                        hw.motherboardManufacturer = key.GetValue("VendorIdentifier")?.ToString() ?? "Unknown Vendor";

                        if (hw.cpu != "Unknown CPU")
                            cpuInfoRetrieved = true; 
                    }
                }
            }
            catch (Exception)
            {         
            }
        }

        // If all methods failed, show the message box
        if (!cpuInfoRetrieved)
        {
            MessageBox.Show("Failed to retrieve CPU information!");
            hw.cpu = "Unknown CPU";
            hw.cores = 0;
            hw.threads = 0;
        }

        // Query GPU information (find the GPU with the highest VRAM)
        try
        {
            searcher = new ManagementObjectSearcher("select * from Win32_VideoController");
            ulong maxVRAM = 0;
            string fallbackGPU = "Unknown GPU";
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["Name"] != null && string.IsNullOrEmpty(fallbackGPU))
                    fallbackGPU = obj["Name"].ToString();  // Set fallback to first found GPU name

                if (obj["AdapterRAM"] != null && obj["Name"] != null)
                {
                    ulong currentVRAM = Convert.ToUInt64(obj["AdapterRAM"]);
                    if (currentVRAM > maxVRAM)
                    {
                        maxVRAM = currentVRAM;
                        hw.gpu = obj["Name"].ToString();
                    }
                }
            }

            if (string.IsNullOrEmpty(hw.gpu))
                hw.gpu = fallbackGPU;

            if (hw.gpu == "Unknown GPU")
                MessageBox.Show("The GPU did not return data correctly!");
        }
        catch
        {
            MessageBox.Show("Failed to retrieve GPU information!");
            hw.gpu = "Unknown GPU";
        }

        // Query RAM information
        try
        {
            searcher = new ManagementObjectSearcher("select * from Win32_PhysicalMemory");
            ulong totalRam = 0;
            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["Capacity"] != null)
                    totalRam += Convert.ToUInt64(obj["Capacity"]);
            }
            hw.ram = (int)(totalRam / (1024 * 1024 * 1024));

            if (hw.ram == 0)
                MessageBox.Show("The RAM did not return data correctly!");
        }
        catch
        {
            MessageBox.Show("Failed to retrieve RAM information!");
            hw.ram = 0;
        }

        // Query OS information
        hw.osInfo = "Unknown OS";
        hw.osArchitecture = "Unknown Architecture";
        try
        {
            searcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                try
                {
                    hw.osInfo = obj["Caption"]?.ToString() ?? "Unknown OS";
                    hw.osArchitecture = obj["OSArchitecture"]?.ToString() ?? "Unknown Architecture";
                }
                catch (ManagementException)
                {
                    Debug.Print("Failed to get object from WMI");
                }
            }

            if (hw.osArchitecture != "Unknown Architecture") // We've got architecture from WMI
            {
                Debug.Print("Got architecture from WMI");

                // Extract numeric portion from OSArchitecture (e.g., "64-bit")
                string numericPortion = "";

                foreach (char c in hw.osArchitecture)
                    if (char.IsDigit(c))
                        numericPortion += c;

                hw.osArchitecture = numericPortion + "-bit";
            }
            else // We need to get if from Registry
            {
                Debug.Print("Getting architecture from Registry");

                string RegArchitecture = HKLM_GetString(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment",
                    "PROCESSOR_ARCHITECTURE");
                switch (RegArchitecture)
                {
                    case "AMD64":
                    case "EM64T": // Some early Intel stuff
                        hw.osArchitecture = "64-bit";
                        break;
                    case "x86":
                        hw.osArchitecture = "32-bit";
                        break;
                    default: // ??????
                        hw.osArchitecture = "Unknown Architecture";
                        break;
                }
            }

            if (hw.osInfo == "Unknown OS" || hw.osArchitecture == "Unknown Architecture")
                MessageBox.Show("The operating system did not return data correctly!");

            // Replace Cyrillic writing of "Microsoft" with the Latin one
            // This works for Russian and Ukrainian, idk about other Cyrillic languages
            // Hopefully other alphabets don't do something similar :clueless:
            hw.osInfo = hw.osInfo.Replace("Майкрософт", "Microsoft");
            
            // TODO: Do Wine detection
            // if (GameBase.IsWine)
            //     osInfo = LinuxDetection.GetLinuxDistroInfo();
        }
        catch
        {
            MessageBox.Show("Failed to retrieve OS information!");
        }

        // Query motherboard information
        try
        {
            searcher = new ManagementObjectSearcher("select * from Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                hw.motherboardManufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown Manufacturer";
                hw.motherboard = obj["Product"]?.ToString() ?? "Unknown Motherboard";

                if (hw.motherboardManufacturer == "Unknown Manufacturer" || hw.motherboard == "Unknown Motherboard")
                    MessageBox.Show("The motherboard did not return data correctly!");
            }
        }
        catch
        {
            MessageBox.Show("Failed to retrieve motherboard information!");
            hw.motherboardManufacturer = "Unknown Manufacturer";
            hw.motherboard = "Unknown Motherboard";
        }

        return hw;
    }
}