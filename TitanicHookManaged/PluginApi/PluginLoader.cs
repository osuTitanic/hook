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
        var pluginFiles = Directory.GetFiles("Plugins", "*.dll");

        foreach (var file in pluginFiles)
        {
            try
            {
                var assembly = Assembly.LoadFrom(file);
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
