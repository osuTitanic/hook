#if NET40
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;
using TitanicHookShared;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Hook for Host header in clients using pWebRequest.
/// pWebRequest uses HttpWebRequest.Host to set host
/// </summary>
public static class HostHeaderHook
{
    public const string HookName = "sh.Titanic.Hook.HostHeader";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        var harmony = HarmonyInstance.Create(HookName);
        
        MethodInfo? targetMethod = GetTargetMethod(AssemblyUtils.CommonOrOsuTypes);
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Target method not found");
            return;
        }
        
        Logging.HookStep(HookName,$"Resolved CreateWebRequest: {targetMethod.DeclaringType?.FullName}.{targetMethod.Name}");
        
        var postfix = typeof(HostHeaderHook).GetMethod("CreateRequestPostfix", Constants.HookBindingFlags);

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, null, new HarmonyMethod(postfix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
    }
    
    #region Hook
    
    private static void CreateRequestPostfix(ref HttpWebRequest __result)
    {
        Logging.HookTrigger(HookName);
        if (__result.Host.Contains("ppy.sh"))
        {
            Logging.HookOutput(HookName, $"Replacing ppy.sh domain with {EntryPoint.Config.ServerName} in CreateRequestPostfix");
            __result.Host = __result.Host.Replace("ppy.sh", EntryPoint.Config.ServerName);
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
            Logging.HookError(HookName, "Couldn't find CreateWebRequest");
            return null;
        }
        
        return targetMethod;
    }

    #endregion
}
#endif // NET40
