using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TitanicHookManaged.Helpers.Benchmark;

public class HardwareDetection
{
    private string renderer;
    private string cpu;
    private int cores;
    private int threads;
    private string gpu;
    private int ram;
    private string osInfo;
    private string osArchitecture;
    private string motherboardManufacturer;
    private string motherboard;
    
    private string HKLM_GetString(string path, string key)
    {
	    try
	    {
		    RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
		    if (rk == null) return "";
		    return (string)rk.GetValue(key);
	    }
	    catch { return ""; }
    }

    private void GetHardwareInfo(bool submitFullHardware)
    {
        string rendererInput = "d3d"; // TODO: Get this from config

        if (rendererInput == "opengl")
            renderer = "OpenGL";
        else if (rendererInput == "d3d")
            renderer = "DirectX";
        else
        {
            MessageBox.Show("The renderer did not return data correctly!");
            renderer = "Unknown Renderer";
        }

        if (!submitFullHardware)
            return;

        ManagementObjectSearcher searcher;

        bool cpuInfoRetrieved = false;

        // Try to query CPU information with WMI
        try
        {
            searcher = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpu = obj["Name"]?.ToString() ?? "Unknown CPU";

                // Use `NumberOfCores` but fallback to `NumberOfLogicalProcessors` if unavailable
                cores = obj["NumberOfCores"] != null ? Convert.ToInt32(obj["NumberOfCores"]) :
                        (obj["NumberOfLogicalProcessors"] != null ? Convert.ToInt32(obj["NumberOfLogicalProcessors"]) / 2 : 0); // Guess 2 cores for older processors

                // Use `ThreadCount` if available, otherwise fallback to `NumberOfLogicalProcessors`
                threads = obj["ThreadCount"] != null ? Convert.ToInt32(obj["ThreadCount"]) :
                        (obj["NumberOfLogicalProcessors"] != null ? Convert.ToInt32(obj["NumberOfLogicalProcessors"]) : 0);

                if (cpu != "Unknown CPU" && cores > 0 && threads > 0)
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
                    cpu = firstCPU.Name;
                    cores = (int)firstCPU.Cores;
                    threads = (int)firstCPU.LogicalCpus;

                    if (cpu != "Unknown CPU" && cores > 0 && threads > 0)
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
                        cpu = key.GetValue("ProcessorNameString")?.ToString() ?? "Unknown CPU";
                        motherboard = key.GetValue("Identifier")?.ToString() ?? "Unknown Identifier";
                        motherboardManufacturer = key.GetValue("VendorIdentifier")?.ToString() ?? "Unknown Vendor";

                        if (cpu != "Unknown CPU")
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
            cpu = "Unknown CPU";
            cores = 0;
            threads = 0;
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
                        gpu = obj["Name"].ToString();
                    }
                }
            }

            if (string.IsNullOrEmpty(gpu))
                gpu = fallbackGPU;

            if (gpu == "Unknown GPU")
                MessageBox.Show("The GPU did not return data correctly!");
        }
        catch
        {
            MessageBox.Show("Failed to retrieve GPU information!");
            gpu = "Unknown GPU";
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
            ram = (int)(totalRam / (1024 * 1024 * 1024));

            if (ram == 0)
                MessageBox.Show("The RAM did not return data correctly!");
        }
        catch
        {
            MessageBox.Show("Failed to retrieve RAM information!");
            ram = 0;
        }

        // Query OS information
        osInfo = "Unknown OS";
        osArchitecture = "Unknown Architecture";
        try
        {
            searcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                try
                {
                    osInfo = obj["Caption"]?.ToString() ?? "Unknown OS";
                    osArchitecture = obj["OSArchitecture"]?.ToString() ?? "Unknown Architecture";
                }
                catch (ManagementException)
                {
                    Debug.Print("Failed to get object from WMI");
                }
            }

            if (osArchitecture != "Unknown Architecture") // We've got architecture from WMI
            {
                Debug.Print("Got architecture from WMI");

                // Extract numeric portion from OSArchitecture (e.g., "64-bit")
                string numericPortion = "";

                foreach (char c in osArchitecture)
                    if (char.IsDigit(c))
                        numericPortion += c;

                osArchitecture = numericPortion + "-bit";
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
                        osArchitecture = "64-bit";
                        break;
                    case "x86":
                        osArchitecture = "32-bit";
                        break;
                    default: // ??????
                        osArchitecture = "Unknown Architecture";
                        break;
                }
            }

            if (osInfo == "Unknown OS" || osArchitecture == "Unknown Architecture")
                MessageBox.Show("The operating system did not return data correctly!");

            // Replace Cyrillic writing of "Microsoft" with the Latin one
            // This works for Russian and Ukrainian, idk about other Cyrillic languages
            // Hopefully other alphabets don't do something similar :clueless:
            osInfo = osInfo.Replace("Майкрософт", "Microsoft");
            
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
                motherboardManufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown Manufacturer";
                motherboard = obj["Product"]?.ToString() ?? "Unknown Motherboard";

                if (motherboardManufacturer == "Unknown Manufacturer" || motherboard == "Unknown Motherboard")
                    MessageBox.Show("The motherboard did not return data correctly!");
            }
        }
        catch
        {
            MessageBox.Show("Failed to retrieve motherboard information!");
            motherboardManufacturer = "Unknown Manufacturer";
            motherboard = "Unknown Motherboard";
        }
    }
}