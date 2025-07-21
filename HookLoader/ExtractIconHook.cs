using System;
using System.Drawing;
using System.Reflection;
using System.Linq;
using Harmony;

namespace HookLoader;

public static class ExtractIconHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.ExtractIconHook");
        
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
        
        var prefix = typeof(ExtractIconHook).GetMethod("ExtractAssociatedIconPrefix", BindingFlags.Static | BindingFlags.Public);

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
    
    public static void ExtractAssociatedIconPrefix(ref string __0)
    {
        __0 = __0.Replace("HookLoader.exe", "osu!.exe");
    }
    
    #endregion
}
