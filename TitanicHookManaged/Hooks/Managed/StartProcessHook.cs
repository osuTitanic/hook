using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Harmony;
using TitanicHookShared;

namespace TitanicHookManaged.Hooks.Managed;

public static class StartProcessHook
{
    public const string HookName = "sh.Titanic.Hook.StartProcess";
    
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create(HookName);
        
        MethodInfo? targetMethod = typeof(Process)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "Start" &
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType.FullName == "System.Diagnostics.ProcessStartInfo");

        if (targetMethod == null)
        {
            Console.WriteLine("Couldn't find Process.Start(ProcessStartInfo)");
            return;
        }

        var prefix = typeof(StartProcessHook).GetMethod("ProcessStartPrefix", Constants.HookBindingFlags);

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

    private static void ProcessStartPrefix(ref ProcessStartInfo __0)
    {
        Console.WriteLine("ProcessStartPrefix triggered");
        if (__0.FileName.Contains("ppy.sh")) // TODO: Make regex check for URLs
        {
            __0.FileName = __0.FileName.Replace("ppy.sh", EntryPoint.Config.ServerName);
        }
    }

    #endregion
}
