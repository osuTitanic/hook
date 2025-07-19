using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public static class CheckCertificateHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.titanic.hook.hostheaderhook");
        
        // Check if osu!common is present
        Assembly? targetAssembly = AssemblyUtils.GetAssembly("osu!common");
        if (targetAssembly == null)
        {
            // If not, get the osu! assembly
            targetAssembly = AssemblyUtils.GetAssembly("osu!");
        }

        if (targetAssembly == null)
        {
            Console.WriteLine("Target assembly not found");
            return;
        }

        MethodInfo? targetMethod = null;
        
        // Workaround is needed in case reflection can't load an osu!.exe dependency
        List<Type> loadedTypes = new List<Type>();
        try
        {
            // Try to load normally
            loadedTypes.AddRange(targetAssembly.GetTypes());
        }
        catch (ReflectionTypeLoadException e)
        {
            // It failed so we start over again but this time we only include the valid types
            loadedTypes = new List<Type>(); // wipe
            loadedTypes.AddRange(e.Types.Where(t => t != null).ToList());
        }

        targetMethod = GetTargetMethod(loadedTypes.ToArray());
        
        if (targetMethod == null)
        {
            Console.WriteLine("Target method not found");
            return;
        }
        Console.WriteLine($"Resolved checkCertificate: {targetMethod.DeclaringType?.FullName}.{targetMethod.Name}");
        
        var prefix = typeof(CheckCertificateHook).GetMethod("CheckCertificatePrefix", BindingFlags.Static | BindingFlags.Public);

        try
        {
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Hook fail: {e}");
        }
    }

    public static bool CheckCertificatePrefix()
    {
        // Do not check the certificate and return early and don't give control back to original function
        Console.WriteLine("checkCertificate prefix triggered");
        return false;
    }
    
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
        
        // Private method with 0 args returning void
        // Virtualized methods define an object[] and a predictable string
        MethodInfo? targetMethod = pWebRequestClass
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m => m.GetParameters().Length == 0 &&
                                 m.ReturnType.FullName == "System.Void" &&
                                 (IsVirtualized(m) || m.Name == "checkCertificate") // Full name fallback for deobfuscated clients
                                 );

        if (targetMethod == null)
        {
            Console.WriteLine("Couldn't find checkCertificate");
            return null;
        }
        
        return targetMethod;
    }

    /// <summary>
    /// Check whether the method is virtualized using Eazfuscator
    /// </summary>
    /// <param name="targetMethod"></param>
    /// <returns></returns>
    private static bool IsVirtualized(MethodInfo? targetMethod)
    {
        int objArrDefCount = 0;
        int virtStringDefCount = 0;
        try
        {
            ILReader reader = new ILReader(targetMethod);

            foreach (ILInstruction instr in reader)
            {
                if (instr.OpCode == OpCodes.Newarr &&
                    instr is InlineTypeInstruction { Type.FullName: "System.Object" })
                {
                    objArrDefCount++;
                }

                // TODO: Add some check if it's actually an Eazfuscator
                if (instr.OpCode == OpCodes.Ldstr //&&
                    //instr is InlineStringInstruction instrStr
                   )
                {
                    virtStringDefCount++;
                }
            }
        }
        catch
        {
            // ignore
        }
        
        return objArrDefCount == 1 && virtStringDefCount == 1;
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

        return servicePointCallCount == 3 || servicePointCallCount == 4;
    }
}
