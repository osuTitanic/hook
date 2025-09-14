using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;

namespace TitanicHookManaged.Helpers.Benchmark;

public static class HardwareFinder
{
    public static List<CPU> GetCPUs()
    {
        List<CPU> cpus = new List<CPU>();
        CPU cpu = null;

        // Try primary method: WMI with System.Management
        try
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                cpu = new CPU
                {
                    Name = Convert.ToString(obj.Properties["Name"].Value),
                    Cores = Convert.ToUInt32(obj.Properties["NumberOfCores"].Value),
                    LogicalCpus = Convert.ToUInt32(obj.Properties["NumberOfLogicalProcessors"].Value),
                };

                if (obj.Properties["ThreadCount"] != null)
                    cpu.Threads = Convert.ToUInt32(obj.Properties["ThreadCount"].Value);

                cpus.Add(cpu);
            }

            if (cpus.Count > 0)
                return cpus; // Success using WMI
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Primary method failed: {ex.Message}");
        }

        // If WMI fails, use secondary backup method
        try
        {
            return GetCPUsFromIndicium();
        }
        catch (Exception ex)
        {
            Debug.Print($"Secondary method failed: {ex.Message}");
        }

        // If both methods fail, fallback to registry-based method
        try
        {
            return GetCPUsFromRegistry();
        }
        catch (Exception ex)
        {
            Debug.Print($"Registry method failed: {ex.Message}");
        }

        return cpus; // In case all methods fail, return an empty list
    }

    // Secondary Backup Method: Indicium-like implementation
    // Implementation based off of https://github.com/hellzerg/optimizer/blob/46e056f6a943b34050a8fe24a0a55e279d732401/Optimizer/IndiciumHelper.cs#L8
    private static List<CPU> GetCPUsFromIndicium()
    {
        List<CPU> cpus = new List<CPU>();

        ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
        foreach (ManagementObject mo in searcher.Get())
        {
            CPU cpu = new CPU
            {
                Name = Convert.ToString(mo.Properties["Name"].Value),
                Cores = Convert.ToUInt32(mo.Properties["NumberOfCores"].Value),
                LogicalCpus = Convert.ToUInt32(mo.Properties["NumberOfLogicalProcessors"].Value),
                Stepping = Convert.ToString(mo.Properties["Description"].Value),
            };

            cpus.Add(cpu);
        }

        return cpus;
    }

    // Third Method (Fallback): Retrieve CPU info from the registry
    private static List<CPU> GetCPUsFromRegistry()
    {
        List<CPU> cpus = new List<CPU>();
        try
        {
            string registryKey = @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                if (key != null)
                {
                    CPU cpu = new CPU
                    {
                        Name = key.GetValue("ProcessorNameString")?.ToString() ?? "Unknown CPU",
                        Stepping = key.GetValue("Identifier")?.ToString() ?? "Unknown Stepping"
                    };

                    // Threads and cores will need to be estimated
                    // We can use logical CPUs as a guess (threads)
                    cpu.LogicalCpus = Convert.ToUInt32(key.GetValue("~MHz")?.ToString() ?? "0"); // Just an example

                    cpus.Add(cpu);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to retrieve CPU information from the registry: {ex.Message}");
        }

        return cpus;
    }
}

// CPU Class to hold CPU information
public class CPU
{
    public string Name { get; set; }
    public uint Cores { get; set; }
    public uint LogicalCpus { get; set; }
    public uint Threads { get; set; }  // ThreadCount, if available
    public string Stepping { get; set; }
}
