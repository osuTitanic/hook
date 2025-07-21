using System;
using System.Reflection;
using Harmony;

namespace HookLoader;

/// <summary>
/// Hook to load our own hooks after osu!.exe finishes loading but before main is called
/// </summary>
public static class OsuStartHook
{
    public static void Initialize(MethodInfo method)
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.OsuStartHook");
        var prefix = typeof(OsuStartHook).GetMethod("OsuStartPrefix", BindingFlags.Static | BindingFlags.Public);
        try
        {
            harmony.Patch(method, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Hook fail: {e}");
        }
        Console.WriteLine("OsuStartHook initialized.");
    }
    
    #region Hook
    
    public static void OsuStartPrefix()
    {
        // Load TitanicHook
        TitanicHookManaged.EntryPoint.InitializeHooks();
    }
    
    #endregion
}
