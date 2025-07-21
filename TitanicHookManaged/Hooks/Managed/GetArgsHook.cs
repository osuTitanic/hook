using System;
using System.Reflection;
using Harmony;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for spoofing GetCommandLineArgs.
/// Only to be used by HookLoader
/// </summary>
public static class GetArgsHook
{
    private static string[] _spoofedArgs;
    
    public static void Initialize(string[] spoofedArgs)
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.GetArgsHook");
        _spoofedArgs = spoofedArgs;
        
        MethodInfo? targetMethod = typeof(Environment).GetMethod("GetCommandLineArgs", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Console.WriteLine("Could not find GetCommandLineArgs");
            return;
        }
        
        var prefix = typeof(GetArgsHook).GetMethod("GetArgsPrefix", BindingFlags.Static | BindingFlags.Public);
        
        try
        {
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Hook fail: {e}");
        }
    }

    #region Hook

    public static bool GetArgsPrefix(ref string[] __result)
    {
        Console.WriteLine("GetCommandLineArgs hook triggered");
        __result = _spoofedArgs;
        return false;
    }

    #endregion
    
}
