using System;
using System.Reflection;
using Harmony;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for spoofing GetEntryAssembly.
/// Only to be used in HookLoader
/// </summary>
public static class EntryPointHook
{
    private static Assembly? _spoofedEntryPointAssembly;
    
    public static void Initialize(Assembly? spoofedAssembly)
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.GetEntryAssemblyHook");
        _spoofedEntryPointAssembly = spoofedAssembly;

        MethodInfo? targetMethod = typeof(Assembly).GetMethod("GetEntryAssembly", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Console.WriteLine("Could not find entry assembly target method");
            return;
        }
        
        var prefix = typeof(EntryPointHook).GetMethod("GetEntryAssemblyPrefix", Constants.HookBindingFlags);

        try
        {
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Hook fail: {e}");
        }
        Console.WriteLine("Entry point hooked");
    }
    
    #region Hook
    
    private static bool GetEntryAssemblyPrefix(ref Assembly? __result)
    {
        Console.WriteLine("GetEntryAssembly hook triggered");
        __result = _spoofedEntryPointAssembly;
        return false;
    }
    
    #endregion
}
