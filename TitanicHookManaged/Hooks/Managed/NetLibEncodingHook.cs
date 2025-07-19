using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ClrTest.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

public static class NetLibEncodingHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.NetLibEncoding");

        ConstructorInfo? targetMethod = GetTargetMethod(AssemblyUtils.OsuOrCommonTypes);
        if (targetMethod == null)
        {
            Console.WriteLine("Target method not found");
            return;
        }
        
        Console.WriteLine($"Resolved StringStream ctor: {targetMethod.DeclaringType.FullName}.{targetMethod.Name}");
        
        var postfix = typeof(NetLibEncodingHook).GetMethod("StringStreamCtorPostfix", BindingFlags.Static | BindingFlags.Public);

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

    /// <summary>
    /// Hook for StringStream constructor, that forces writing with UTF-8 without BOM encoding
    /// This has to be a postfix, due to __instance not being initialized yet when it's a prefix
    /// </summary>
    /// <param name="__instance">StringStream instance, here we are taking it as MemoryStream due to StringStream being a custom type that implements MemoryStream</param>
    /// <param name="__0">text</param>
    /// <param name="__1">args</param>
    public static void StringStreamCtorPostfix(ref MemoryStream __instance, string __0, params object[] __1)
    {
        Console.WriteLine("StringStream ctor hook triggered");
        try
        {
            __instance.SetLength(0); // Wipe the underlying MemoryStream without disposing it
            
            // Replicate the behavior of original constructor the right way
            StreamWriter sw = new StreamWriter(__instance, new UTF8Encoding(false));
            if (__1.Length == 0)
            {
                sw.Write(__0);
            }
            else
            {
                sw.Write(__0, __1);
            }
        
            sw.Flush();
            __instance.Seek(0, SeekOrigin.Begin);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in StringStream ctor hook: {e}");
        }
    } 
    
    #endregion
    
    #region Find method

    /// <summary>
    /// Find target method to hook
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    private static ConstructorInfo? GetTargetMethod(Type[] types)
    {
        var validMethods = types
            .SelectMany(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            .Where(m => m.GetParameters().Length == 2 &&
                        m.GetParameters()[0].ParameterType.FullName == "System.String" &&
                        m.GetParameters()[1].ParameterType.FullName == "System.Object[]" &&
                        (UsesStreamWriter(m) || m.DeclaringType?.Name == "StringStream") // hardcoded name fallback for clients using Confuser (b20131216)
                        );
        
        return validMethods.FirstOrDefault();
    }

    /// <summary>
    /// Check whether the constructor is using StreamWriter
    /// </summary>
    /// <param name="targetMethod"></param>
    /// <returns></returns>
    private static bool UsesStreamWriter(ConstructorInfo targetMethod)
    {
        try
        {
            ILReader reader = new ILReader(targetMethod);

            foreach (ILInstruction instr in reader)
            {
                if (instr.OpCode == OpCodes.Newobj &&
                    instr is InlineMethodInstruction method && // Newobj opcode will contain a method call to a ctor
                    method.Method.DeclaringType is { Name: "StreamWriter" })
                {
                    return true;
                }
            }
        }
        catch
        {
            // Ignore any errors, hopefully nothing goes wrong
            //Debugger.Break();
        }
        
        return false;
    }
    
    #endregion
}
