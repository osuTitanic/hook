using System;
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

    /// <summary>
    /// Find target method to hook
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private static MethodInfo? GetTargetMethod(Type[] types)
    {
        foreach (var type in types)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();

                if (parameters.Length != 3)
                    continue;
                
                // Not sure why comparing string values is necessary, but otherwise it wouldn't work as expected
                if (parameters[0].ParameterType.FullName != "System.Collections.Specialized.StringCollection" ||
                    parameters[1].ParameterType.FullName != "System.String" ||
                    parameters[2].ParameterType.FullName != "System.String")
                    continue;

                if (method.ReturnType != typeof(void))
                    continue;

                return method;
            }
        }

        return null;
    }

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
}
