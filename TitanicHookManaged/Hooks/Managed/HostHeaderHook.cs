#if NET40
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;
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
        
        MethodInfo? targetMethod = GetTargetMethod(AssemblyUtils.OsuOrCommonTypes);
        if (targetMethod == null)
        {
            Console.WriteLine("Target method not found");
            return;
        }
        
        Console.WriteLine($"Resolved CreateWebRequest: {targetMethod.DeclaringType?.FullName}.{targetMethod.Name}");
        
        var postfix = typeof(HostHeaderHook).GetMethod("CreateRequestPostfix", BindingFlags.Static | BindingFlags.Public);

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
    
    public static void CreateRequestPostfix(ref HttpWebRequest __result)
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
        // Try to find pWebRequest
        // It will have a private static constructor with 0 params and will call ServicePointManager 3 or 4 times
        Type? pWebRequestClass = types
            .SelectMany(t => t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static))
            .Where(m => m.GetParameters().Length == 0 && HasServicePointManagerCalls(m))
            .Select(m => m.DeclaringType)
            .FirstOrDefault();

        if (pWebRequestClass == null)
        {
            Console.WriteLine("Couldn't find pWebRequest");
            return null;
        }
        
        // Protected virtual method with 0 args returning HttpWebRequest
        MethodInfo? targetMethod = pWebRequestClass
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m => m.GetParameters().Length == 0 && m.ReturnType.FullName == "System.Net.HttpWebRequest");

        if (targetMethod == null)
        {
            Console.WriteLine("Couldn't find CreateWebRequest");
            return null;
        }
        
        return targetMethod;
    }
    
    private static bool HasServicePointManagerCalls(ConstructorInfo? targetMethod)
    {
        int servicePointCallCount = 0;
        try
        {
            ILReader reader = new ILReader(targetMethod);

            foreach (ILInstruction instr in reader)
            {
                // Check if it's calling one of the methods that pWebRequest static ctor calls, and add it to the counter if it does
                if (instr.OpCode == OpCodes.Call &&
                    instr is InlineMethodInstruction method &&
                    method.Method.Name is "set_Expect100Continue" or "set_DefaultConnectionLimit" or "set_CheckCertificateRevocationList" or "set_SecurityProtocol")
                {
                    servicePointCallCount++;
                }
            }
        }
        catch
        {
            // ignore
        }

        return servicePointCallCount is 3 or 4;
    }

    #endregion
}
#endif // NET40
