using System;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

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
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);
        _spoofedArgs = spoofedArgs;
        
        MethodInfo? targetMethod = typeof(Environment).GetMethod("GetCommandLineArgs", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Could not find GetCommandLineArgs");
            return;
        }
        
        var prefix = typeof(GetArgsHook).GetMethod("GetArgsPrefix", Constants.HookBindingFlags);
        
        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
    }

    #region Hook

    private static bool GetArgsPrefix(ref string[] __result)
    {
        Logging.HookTrigger(HookName);
        __result = _spoofedArgs;
        return false;
    }

    #endregion
    
}
