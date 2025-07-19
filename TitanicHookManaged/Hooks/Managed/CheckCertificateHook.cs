using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Managed;

public static class CheckCertificateHook
{
    public static void Initialize()
    {
        var harmony = HarmonyInstance.Create("sh.Titanic.Hook.CheckCertificate");

        MethodInfo? targetMethod = GetTargetMethod(AssemblyUtils.OsuOrCommonTypes.ToArray());
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
    
    #region Hook

    public static bool CheckCertificatePrefix()
    {
        // Do not check the certificate and return early and don't give control back to original function
        Console.WriteLine("checkCertificate prefix triggered");
        return false;
    }
    
    #endregion
    
    #region Find method
    
    private static MethodInfo? GetTargetMethod(Type[] types)
    {
        // Private method with 0 args returning void, potentially virtualized with Eazfuscator
        MethodInfo? targetMethod = pWebRequestHelper.ReflectedType
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
    /// These methods will define a single object[] and a single string with a predictable format (we are not checking format)
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

                // TODO: Add some check if it's actually an Eazfuscator. Not needed tho
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
    
    #endregion
}
