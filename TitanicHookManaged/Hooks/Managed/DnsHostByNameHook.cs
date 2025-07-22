using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Harmony;

namespace TitanicHookManaged.Hooks.Managed;

public static class DnsHostByNameHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.DnsHostByNameHook");
        
        MethodInfo? targetMethod = typeof(Dns)
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == "InternalGetHostByName" && m.GetParameters().Length == 2);

        if (targetMethod == null)
        {
            Console.WriteLine("Failed to find Dns.InternalGetHostByName(string, bool)");
            return;
        }

        var prefix = typeof(DnsHostByNameHook).GetMethod("InternalGetHostByNamePrefix", Constants.HookBindingFlags);

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

    private static void InternalGetHostByNamePrefix(ref string __0)
    {
        Console.WriteLine($"InternalGetHostByNamePrefix triggered with host name {__0}");
        if (__0.Contains("ppy.sh"))
            __0 = __0.Replace("ppy.sh", EntryPoint.Config.ServerName);
        else if (__0 == "peppy.chigau.com")
            __0 = __0.Replace("peppy.chigau.com", $"chigau.{EntryPoint.Config.ServerName}");
    }
    
    #endregion
}
