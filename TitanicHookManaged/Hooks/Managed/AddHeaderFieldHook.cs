using System;
using System.Linq;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

/// <summary>
/// Managed hook that will replace the value of the Host HTTP header with the desired private server domain
/// </summary>
public static class AddHeaderFieldHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.AddHeaderField");

        MethodInfo? targetMethod = GetTargetMethod(AssemblyUtils.OsuOrCommonTypes);
        if (targetMethod == null)
        {
            Console.WriteLine("Target method not found");
            return;
        }
        Console.WriteLine($"Resolved AddHeaderField: {targetMethod.Name}");
        
        var prefix = typeof(AddHeaderFieldHook).GetMethod("AddHeaderFieldPrefix", BindingFlags.Static | BindingFlags.Public);

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
    
    /// <summary>
    /// Hooked AddHeaderField
    /// </summary>
    /// <param name="__1">Name of the header</param>
    /// <param name="__2">Value of the header. It's ref here so that we can get it by reference and modify it</param>
    public static void AddHeaderFieldPrefix(string __1, ref string __2)
    {
        if (__1 == "Host" && __2.Contains("ppy.sh"))
        {
            __2 = __2.Replace("ppy.sh", "titanic.sh");
        }
        Console.WriteLine($"AddHeaderField hook triggered, {__1}: {__2}");
    }
    
    #endregion

    
    #region Find method
    
    /// <summary>
    /// Find target method to hook
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private static MethodInfo? GetTargetMethod(Type[] types)
    {
        MethodInfo? targetMethod = types
            .SelectMany(m => m.GetMethods(BindingFlags.Static | BindingFlags.Public))
            .FirstOrDefault(m => m.GetParameters().Length == 3 &&
                                 m.GetParameters()[0].ParameterType.FullName ==
                                 "System.Collections.Specialized.StringCollection" &&
                                 m.GetParameters()[1].ParameterType.FullName == "System.String" &&
                                 m.GetParameters()[2].ParameterType.FullName == "System.String" &&
                                 m.ReturnType.FullName == "System.Void");
        
        return targetMethod;
    }
    
    #endregion
}
