using System;
using System.Reflection;
using Harmony;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for spoofing Application.ExecutablePath getter.
/// Only to be used by HookLoader
/// </summary>
public static class ExecutablePathHook
{
    private static string? _spoofedExePath;
    
    public static void Initialize(string? spoofedExePath)
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.ExecutablePathHook");
        _spoofedExePath = spoofedExePath;

        MethodInfo? targetMethod = typeof(System.Windows.Forms.Application).GetMethod("get_ExecutablePath", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Console.WriteLine("Could not find get_ExecutablePath method");
            return;
        }
        
        var prefix = typeof(ExecutablePathHook).GetMethod("GetExecutablePathPrefix", Constants.HookBindingFlags);

        try
        {
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Hook fail: {e}");
        }
        Console.WriteLine("Executable path hooked");
    }
    
    #region Hook
    
    private static bool GetExecutablePathPrefix(ref string? __result)
    {
        Console.WriteLine("get_ExecutablePath hook triggered");
        __result = _spoofedExePath;
        return false;
    }
    
    #endregion
}
