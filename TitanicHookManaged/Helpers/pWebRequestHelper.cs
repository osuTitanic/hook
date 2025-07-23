using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;
using TitanicHookShared;

namespace TitanicHookManaged.Helpers;

/// <summary>
/// Utils for pWebRequest (used in multiple hooks so it's easier to split into class)
/// </summary>
public static class pWebRequestHelper
{
    /// <summary>
    /// Reflected pWebRequestType
    /// </summary>
    public static Type? ReflectedType
    {
        get
        {
            _pWebRequestType ??= AssemblyUtils.CommonOrOsuTypes
                .SelectMany(t => t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Static))
                .Where(m => m.GetParameters().Length == 0 &&
                            HasServicePointManagerCalls(m))
                .Select(m => m.DeclaringType)
                .FirstOrDefault();
            
            if (_pWebRequestType == null)
            {
                Logging.Info("Couldn't find pWebRequest");
            }
            
            return _pWebRequestType;
        }
    }
    
    /// <summary>
    /// Check if constructor has 3 or 4 ServicePointManaged calls (property of pWebRequest)
    /// </summary>
    /// <param name="targetMethod"></param>
    /// <returns></returns>
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

    #region Private cache for getters

    private static Type? _pWebRequestType;

    #endregion
}
