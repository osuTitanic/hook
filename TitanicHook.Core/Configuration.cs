// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee
 
using System;
using System.IO;

namespace TitanicHook.Core;

/// <summary>
/// Callback delegate for config not found
/// </summary>
public delegate void ConfigurationNotFoundCallback(string filename, Exception e);

public class Configuration
{
    public const int CurrentConfigVersion = 2;

    /// <summary>
    /// Version of the config file
    /// </summary>
    public int Version { get; set; } = CurrentConfigVersion;

    /// <summary>
    /// Alloc console for logs
    /// </summary>
    public bool EnableConsole { get; set; } = false;

    /// <summary>
    /// Log to a file
    /// </summary>
    public bool LogToFile { get; set; } = false;

    /// <summary>
    /// Domain of the server (e.g. titanic.sh)
    /// </summary>
    public string ServerName { get; set; } = "titanic.sh";

    /// <summary>
    /// Whether to hook WSAConnect (TCP Bancho)
    /// </summary>
    public bool HookTcpConnections { get; set; } = true;

    /// <summary>
    /// Whether to hook NetLib AddHeaderField
    /// </summary>
    public bool HookNetLibHeaders { get; set; } = true;

    /// <summary>
    /// Whether to hook NetLib StringStream ctor
    /// </summary>
    public bool HookNetLibEncoding { get; set; } = true;

    /// <summary>
    /// Whether to hook Host header in pWebRequest (.NET Framework 4.x only)
    /// </summary>
    public bool HookModernHostMethod { get; set; }
#if NET40
         = true;
#else
        = false;
#endif

    /// <summary>
    /// Gets or sets a value indicating whether certificate validation is hooked.
    /// </summary>
    public bool HookCheckCertificate { get; set; }
#if NET40
         = true;
#else
        = false;
#endif
    
    /// <summary>
    /// Whether to allow running under Mono
    /// </summary>
    public bool AllowMono { get; set; } = false;
    
    /// <summary>
    /// SHA256 of client that the config is generated for.
    /// This is to prevent people from sharing config files.
    /// </summary>
    public string ClientSha256 { get; set; } = "";
    
    /// <summary>
    /// osu! adds a delay when loading leaderboards.
    /// This will remove it, and request the leaderboard instantly.
    /// </summary>
    public bool RemoveScoreFetchingDelay { get; set; } = true;

    /// <summary>
    /// Usually, when trying to message peppy, a page would pop up, explaining that peppy is a busy man!
    /// On Titanic!, Levi has the same user id as peppy so this popup would still appear.
    /// We want users to message him, so this option removes it.
    /// </summary>
    public bool RemovePeppyDmCheck { get; set; } = true;

    /// <summary>
    /// Whether to auto-update Titanic!
    /// </summary>
    public bool DisableUpdates { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the launcher is disabled.
    /// </summary>
    public bool DisableLauncher { get; set; } = false;

    /// <summary>
    /// The version of osu! that was downloaded through the launcher.
    /// </summary>
    public int ReleaseVersion { get; set; } = 0;

    /// <summary>
    /// Whether it's the first config creation.
    /// This is not written to the config!
    /// </summary>
    public bool FirstRun { get; set; } = false;

    public string Filename;

    /// <summary>
    /// Parameterless constructor for default config values. Nothing is getting parsed here
    /// </summary>
    public Configuration()
    {
        Filename = Constants.DefaultConfigName;
    }

    /// <summary>
    /// Loads a config from specified file
    /// </summary>
    /// <param name="filename">Path to the target file</param>
    /// <param name="callback">Optional: callback that will be called if the config is not found</param>
    public Configuration(string filename, ConfigurationNotFoundCallback? callback = null)
    {
        Filename = filename;
        string[] lines;
        try
        {
            lines = File.ReadAllLines(filename);
        }
        catch (Exception e)
        {
            callback ??= DefaultConfigNotFoundCallback; // Assign default callback if not specified
            callback(filename, e);
            return;
        }

        foreach (string line in lines)
        {
            string cleanLine = line.Trim();
            if (cleanLine.StartsWith("#") || cleanLine == "")
            {
                continue;
            }
            
            string[] splitLine = cleanLine.Split(['='], 2);
            switch (splitLine[0])
            {
                case "Version":
                    Version = int.Parse(splitLine[1]);
                    if (Version < CurrentConfigVersion)
                    {
                        FirstRun = true;
                        Version = CurrentConfigVersion;
                        return;
                    }
                    break;
                case "EnableConsole":
                    EnableConsole = bool.Parse(splitLine[1]);
                    break;
                case "LogToFile":
                    LogToFile = bool.Parse(splitLine[1]);
                    break;
                case "ServerName":
                    ServerName = splitLine[1];
                    break;
                case "HookTcpConnections":
                    HookTcpConnections = bool.Parse(splitLine[1]);
                    break;
                case "HookNetLibHeaders":
                    HookNetLibHeaders = bool.Parse(splitLine[1]);
                    break;
                case "HookNetLibEncoding":
                    HookNetLibEncoding = bool.Parse(splitLine[1]);
                    break;
                case "HookModernHostMethod":
                    HookModernHostMethod = bool.Parse(splitLine[1]);
                    break;
                case "HookCheckCertificate":
                    HookCheckCertificate = bool.Parse(splitLine[1]);
                    break;
                case "AllowMono":
                    AllowMono = bool.Parse(splitLine[1]);
                    break;
                case "ClientSha256":
                    ClientSha256 = splitLine[1];
                    break;
                case "RemoveScoreFetchingDelay":
                    RemoveScoreFetchingDelay = bool.Parse(splitLine[1]);
                    break;
                case "RemovePeppyDmCheck":
                    RemovePeppyDmCheck = bool.Parse(splitLine[1]);
                    break;
                case "DisableUpdates":
                    DisableUpdates = bool.Parse(splitLine[1]);
                    break;
                case "DisableLauncher":
                    DisableLauncher = bool.Parse(splitLine[1]);
                    break;
                case "ReleaseVersion":
                    ReleaseVersion = int.Parse(splitLine[1]);
                    break;
            }
        }
    }

    /// <summary>
    /// Save current configuration to a file
    /// </summary>
    /// <param name="filename">Config filename</param>
    public void SaveConfiguration(string filename)
    {
        using FileStream fs = new FileStream(filename, FileMode.Create);
        using StreamWriter sw = new StreamWriter(fs);
        
        sw.WriteLine("# Configuration file for Titanic! Hook");
        sw.WriteLine("# If you don't know what you're doing, DO NOT edit this file yourself");
        sw.WriteLine("");
        sw.WriteLine($"Version={Version}");
        sw.WriteLine($"EnableConsole={EnableConsole}");
        sw.WriteLine($"LogToFile={LogToFile}");
        sw.WriteLine($"ServerName={ServerName}");
        sw.WriteLine($"HookTcpConnections={HookTcpConnections}");
        sw.WriteLine($"HookNetLibHeaders={HookNetLibHeaders}");
        sw.WriteLine($"HookNetLibEncoding={HookNetLibEncoding}");
        sw.WriteLine($"HookModernHostMethod={HookModernHostMethod}");
        sw.WriteLine($"HookCheckCertificate={HookCheckCertificate}");
        sw.WriteLine($"AllowMono={AllowMono}");
        sw.WriteLine($"ClientSha256={ClientSha256}");
        sw.WriteLine($"RemoveScoreFetchingDelay={RemoveScoreFetchingDelay}");
        sw.WriteLine($"RemovePeppyDmCheck={RemovePeppyDmCheck}");
        sw.WriteLine($"DisableUpdates={DisableUpdates}");
        sw.WriteLine($"DisableLauncher={DisableLauncher}");
        sw.WriteLine($"ReleaseVersion={ReleaseVersion}");
    }

    /// <summary>
    /// Default callback function for when config is not found
    /// </summary>
    /// <param name="filename">Config filename</param>
    /// <param name="e">Exception thrown when loading the config</param>
    private void DefaultConfigNotFoundCallback(string filename, Exception e)
    {
        Console.WriteLine("Couldn't load TitanicHook config. A default one will be created.");
        FirstRun = true;
    }
}
