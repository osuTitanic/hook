using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for ExtractAssociatedIcon so that osu! will have correct icon.
/// Only to be used in HookLoader
/// </summary>
public static class ExtractIconHook
{
    private static string? _hookLoaderName;
    public const string HookName = "sh.Titanic.Hook.ExtractIcon";
    
    public static void Initialize(string? hookLoaderName)
    {
        Logging.HookStart(HookName);
        
        if (hookLoaderName == null)
            return;
        
        _hookLoaderName = hookLoaderName;
        var harmony = new Harmony(HookName);
        
        // We want specifically the overload that takes System.String
        MethodInfo? targetMethod = typeof(Icon)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "ExtractAssociatedIcon" &&
                                 m.GetParameters().Length == 1 &&
                                 m.GetParameters()[0].ParameterType.FullName == "System.String"
                                 );
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Could not find ExtractAssociatedIcon");
            return;
        }
        
        var prefix = typeof(ExtractIconHook).GetMethod("ExtractAssociatedIconPrefix", Constants.HookBindingFlags);

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
    
    private static void ExtractAssociatedIconPrefix(ref string __0)
    {
        Logging.HookTrigger(HookName);
        __0 = __0.Replace(_hookLoaderName, "osu!.exe"); // Change the target icon path
    }
    
    #endregion
}
