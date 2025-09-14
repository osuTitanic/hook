using System;
using System.Collections.Generic;
using System.IO;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.OsuInterop;

/// <summary>
/// Reader for osu! config
/// </summary>
public class ConfigReader
{
    public ConfigReader(string? filename = null)
    {
        if (filename == null)
        {
            // Try to infer the config name
            string username = Environment.UserName;

            string[] possibleFilenames = [$"osu!.{username}.cfg", "osu!.cfg"];
            foreach (var fn in possibleFilenames)
            {
                try
                {
                    ReadConfig(fn);
                    return;
                }
                catch (Exception e)
                {
                    Logging.Info($"Failed to read {fn}");
                }
            }

            return;
        }
        
        ReadConfig(filename);
    }

    public string TryGetValue(string key)
    {
        kvp.TryGetValue(key, out string value);
        if (value == null)
            value = "";
        return value;
    }

    private void ReadConfig(string filename)
    {
        string[] lines = File.ReadAllLines(filename);
        foreach (var line in lines)
        {
            string[] parts = line.Split('=');
            string key = parts[0].Trim();
            string value = parts[1].Trim();
            
            kvp[key] = value;
        }

        Logging.Info($"Read osu! config from {filename}");
    }

    public Dictionary<string, string> kvp = new();
}
