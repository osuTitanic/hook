using System;
using System.Reflection;
using Harmony;
using TitanicHookShared;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for spoofing GetCommandLineArgs.
/// Only to be used by HookLoader
/// </summary>
public static class GetArgsHook
{
    public const string HookName = "sh.Titanic.Hook.GetArgs";
    private static string[] _spoofedArgs;
    
    public static void Initialize(string[] spoofedArgs)
    {
        var harmony = HarmonyInstance.Create(HookName);
        _spoofedArgs = spoofedArgs;
        
        MethodInfo? targetMethod = typeof(Environment).GetMethod("GetCommandLineArgs", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Console.WriteLine("Could not find GetCommandLineArgs");
            return;
        }
        
        var prefix = typeof(GetArgsHook).GetMethod("GetArgsPrefix", Constants.HookBindingFlags);
        
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

    private static bool GetArgsPrefix(ref string[] __result)
    {
        Console.WriteLine("GetCommandLineArgs hook triggered");
        __result = _spoofedArgs;
        return false;
    }

    #endregion
    
}
