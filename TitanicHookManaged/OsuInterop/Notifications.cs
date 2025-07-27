using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.OsuInterop;

public static class Notifications
{
    #region Signatures

    /// <summary>
    /// Exact signature for ShowMessage (works for most methods)
    /// </summary>
    static OpCode[] _showMessageSig =
    [
        OpCodes.Newobj,
        OpCodes.Stloc_0,
        OpCodes.Ldloc_0,
        OpCodes.Ldarg_0,
        OpCodes.Stfld,
        OpCodes.Ldsfld,
        OpCodes.Ldloc_0,
        OpCodes.Ldftn,
        OpCodes.Newobj,
        OpCodes.Ldc_I4_1,
        OpCodes.Callvirt,
        OpCodes.Ret
    ];
    
    /// <summary>
    /// Exact signature for ShowMessage (2016)
    /// </summary>
    static OpCode[] _showMessageSig2016 =
    [
        OpCodes.Newobj,
        OpCodes.Stloc_0,
        OpCodes.Ldloc_0,
        OpCodes.Ldarg_0,
        OpCodes.Stfld,
        OpCodes.Ldsfld,
        OpCodes.Ldloc_0,
        OpCodes.Ldftn,
        OpCodes.Newobj,
        OpCodes.Ldc_I4_1,
        OpCodes.Callvirt,
        OpCodes.Pop,
        OpCodes.Ret
    ];

    #endregion
    
    /// <summary>
    /// Reference to ShowMessage
    /// </summary>
    private static MethodInfo? _showMessageMethodReference = AssemblyUtils.OsuTypes
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
        .FirstOrDefault(m =>
            m.GetParameters().Length == 1 &&
            m.GetParameters()[0].ParameterType.FullName == "System.String" &&
            m.ReturnType.FullName == "System.Void" &&
            SigScanning.CompareMultipleSigs(m, [
                _showMessageSig, 
                _showMessageSig2016
            ])
    );
    
    /// <summary>
    /// Calls ShowMessage
    /// </summary>
    /// <param name="message">Message to show</param>
    public static void ShowMessage(string message) => _showMessageMethodReference?.Invoke(null, [message]);
}
