using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TitanicHook.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.PluginApi;

/// <summary>
/// Manager for plugins that handles loading and their lifecycle
/// </summary>
public class PluginLoader
{
    /// <summary>
    /// Loads all plugins from the default directory
    /// </summary>
    /// <returns>List of plugin objects</returns>
    public static List<IPlugin> LoadPlugins()
    {
        List<IPlugin> plugins = [];
        
        // TODO: Directory.EnumerateFiles is allegedly faster, but it requires .NET Framework 4.
        // Might put that behind an ifdef?
        var manifestFiles = Directory.GetFiles("HookPlugins", "PluginManifest.txt", SearchOption.AllDirectories);

        foreach (var manifest in manifestFiles)
        {
            try
            {
                // Read the manifest to find which dll is the plugin file
                string pluginFileName = File.ReadAllText(manifest).Trim();
                
                var assembly = Assembly.LoadFrom(Directory.GetParent(manifest).FullName + "\\" + pluginFileName);
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface);

                foreach (var pluginType in pluginTypes)
                {
                    var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                    plugin.Initialize(new PluginHost());
                    plugins.Add(plugin);
                }
            }
            catch (Exception e)
            {
                Logging.Info("Failed to load plugin");
            }
        }
        
        return plugins;
    }
}
