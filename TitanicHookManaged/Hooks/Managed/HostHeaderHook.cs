#if NET40
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for Host header in clients using pWebRequest.
/// pWebRequest uses HttpWebRequest.Host to set host
/// </summary>
public static class HostHeaderHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.HostHeader");
        
        MethodInfo? targetMethod = GetTargetMethod(AssemblyUtils.CommonOrOsuTypes);
        if (targetMethod == null)
        {
            Console.WriteLine("Target method not found");
            return;
        }
        
        Console.WriteLine($"Resolved CreateWebRequest: {targetMethod.DeclaringType?.FullName}.{targetMethod.Name}");
        
        var postfix = typeof(HostHeaderHook).GetMethod("CreateRequestPostfix", Constants.HookBindingFlags);

        try
        {
            harmony.Patch(targetMethod, null, new HarmonyMethod(postfix));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Hook fail: {e}");
        }
    }
    
    #region Hook
    
    private static void CreateRequestPostfix(ref HttpWebRequest __result)
    {
        Console.WriteLine($"Triggered CreateRequest postfix: {__result.Host}");
        if (__result.Host.Contains("ppy.sh"))
        {
            Console.WriteLine("Replacing ppy.sh domain in CreateRequestPostfix");
            __result.Host = __result.Host.Replace("ppy.sh", "titanic.sh");
        }
    }
    
    #endregion
    
    #region Find method

    private static MethodInfo? GetTargetMethod(Type[] types)
    {
        // Protected virtual method with 0 args returning HttpWebRequest
        MethodInfo? targetMethod = pWebRequestHelper.ReflectedType
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m => m.GetParameters().Length == 0 && m.ReturnType.FullName == "System.Net.HttpWebRequest");

        if (targetMethod == null)
        {
            Console.WriteLine("Couldn't find CreateWebRequest");
            return null;
        }
        
        return targetMethod;
    }

    #endregion
}
#endif // NET40
