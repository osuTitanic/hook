using System;
using System.Reflection;
using System.Text;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Unused hook for default encoding; did not work, to be removed
/// </summary>
public static class DefaultEncodingHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.titanic.hook.defaultencodinghook");
        
        Assembly? targetAssembly = AssemblyUtils.GetAssembly("mscorlib");
        if (targetAssembly == null)
        {
            Console.WriteLine("Target assembly not found");
            return;
        }
        
        Type type = targetAssembly.GetType("System.Text.Encoding");
        if (type == null)
        {
            Console.WriteLine("Couldn't find System.Text.Encoding");
            return;
        }
        MethodInfo? targetMethod = type.GetMethod("get_Default", BindingFlags.Static | BindingFlags.Public);
        if (targetMethod == null)
        {
            Console.WriteLine("Target method not found");
            return;
        }
        
        Console.WriteLine($"Resolved System.Text.Encoding.get_Default: {targetMethod.DeclaringType.FullName}.{targetMethod.Name}");
        
        var prefix = typeof(DefaultEncodingHook).GetMethod("GetDefaultEncodingPrefix", BindingFlags.Static | BindingFlags.Public);
        var postfix = typeof(DefaultEncodingHook).GetMethod("GetDefaultEncodingPostfix", BindingFlags.Static | BindingFlags.Public);

        try
        {
            harmony.Patch(targetMethod, null, new HarmonyMethod(postfix));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Hook fail: {e}");
        }
    }
    
    private static void GetDefaultEncodingPostfix(ref Encoding __result)
    {
        Console.WriteLine("Default encoding postfix hook triggered");
        __result = new UTF8Encoding(false); // return UTF-8 without BOM
    }

    private static bool GetDefaultEncodingPrefix(ref Encoding __result)
    {
        Console.WriteLine("Default encoding hook triggered");
        __result = new UTF8Encoding(false); // return UTF-8 without BOM
        return false; // do not return control to the original method
    }
}
