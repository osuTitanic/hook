using System;
using System.Collections.Specialized;
using System.Reflection;
using Harmony;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Managed hook that will replace the value of the Host HTTP header with the desired private server domain
/// </summary>
public static class AddHeaderFieldHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.titanic.hook.addheaderfieldhook");
        
        Assembly targetAssembly = null;
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == "osu!common")
            {
                targetAssembly = assembly;
                break;
            }
        }

        if (targetAssembly == null)
        {
            throw new Exception("Target assembly not found");
        }
        Type type = targetAssembly.GetType("osu_common.Libraries.NetLib.HeaderFieldList");
        if (type == null)
        {
            throw new Exception("Can't get header field list class");
        }
        
        MethodInfo? method = type.GetMethod("AddHeaderField", BindingFlags.Static | BindingFlags.Public, null, [typeof(StringCollection), typeof(string), typeof(string)
        ], null);
        if (method == null)
        {
            throw new Exception("Can't get header field list method");
        }
        
        var prefix = typeof(AddHeaderFieldHook).GetMethod("AddHeaderFieldPrefix", BindingFlags.Static | BindingFlags.Public);
        
        harmony.Patch(method, new HarmonyMethod(prefix));
    }

    /// <summary>
    /// Hooked AddHeaderField
    /// </summary>
    /// <param name="source">List of headers</param>
    /// <param name="name">Name of the header</param>
    /// <param name="theValue">Value of the header. It's ref here so that we can get it by reference and modify it</param>
    public static void AddHeaderFieldPrefix(StringCollection source, string name, ref string theValue)
    {
        if (name == "Host" && theValue.Contains("ppy.sh"))
        {
            theValue = theValue.Replace("ppy.sh", "titanic.sh");
        }
        Console.WriteLine($"AddHeaderField hook triggered, {name}: {theValue}");
    }
}
