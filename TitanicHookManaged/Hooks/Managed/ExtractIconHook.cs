using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using Harmony;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for ExtractAssociatedIcon so that osu! will have correct icon.
/// Only to be used in HookLoader
/// </summary>
public static class ExtractIconHook
{
    private static string? _hookLoaderName;
    
    public static void Initialize(string? hookLoaderName)
    {
        if (hookLoaderName == null)
            return;
        
        _hookLoaderName = hookLoaderName;
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.ExtractIconHook");
        
        // We want specifically the overload that takes System.String
        MethodInfo? targetMethod = typeof(Icon)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "ExtractAssociatedIcon" &&
                                 m.GetParameters().Length == 1 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.String"
                                 );
        if (targetMethod == null)
        {
            Console.WriteLine("Could not find ExtractAssociatedIcon");
            return;
        }
        
        var prefix = typeof(ExtractIconHook).GetMethod("ExtractAssociatedIconPrefix", Constants.HookBindingFlags);

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
    
    private static void ExtractAssociatedIconPrefix(ref string __0)
    {
        Console.WriteLine("ExtractAssociatedIcon hook triggered");
        __0 = __0.Replace(_hookLoaderName, "osu!.exe"); // Change the target icon path
    }
    
    #endregion
}
