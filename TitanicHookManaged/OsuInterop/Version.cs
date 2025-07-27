using System.Reflection;
using System.Reflection.Emit;
using TitanicHookManaged.Helpers;
using System.Linq;

namespace TitanicHookManaged.OsuInterop;

public static class Version
{
    /// <summary>
    /// 2015 signature for deobfuscated builds
    /// </summary>
    private static OpCode[] _getVersionSignature =
    [
        OpCodes.Nop,
        OpCodes.Ldstr,
        OpCodes.Ldsfld,
        OpCodes.Box,
        OpCodes.Call,
        OpCodes.Ret
    ];
    
    /// <summary>
    /// Old signature for deobfuscated builds
    /// </summary>
    private static OpCode[] _getVersionSignatureOld =
    [
        OpCodes.Ldstr,
        OpCodes.Ldc_I4,
        OpCodes.Stloc_0,
        OpCodes.Ldloca_S,
        OpCodes.Call,
        OpCodes.Ldsfld,
        OpCodes.Ldsfld,
        OpCodes.Call,
        OpCodes.Ret
    ];
    
    /// <summary>
    /// Old old signature for deobfuscated builds
    /// </summary>
    private static OpCode[] _getVersionSignatureOldOld =
    [
        OpCodes.Ldstr,
        OpCodes.Ldc_I4,
        OpCodes.Stloc_0,
        OpCodes.Ldloca_S,
        OpCodes.Call,
        OpCodes.Ldsfld,
        OpCodes.Call,
        OpCodes.Ret
    ];
    
    /// <summary>
    /// Old old signature for obfuscated builds
    /// </summary>
    private static OpCode[] _getVersionSignatureOldOldObf =
    [
        OpCodes.Ldc_I4,
        OpCodes.Call,
        OpCodes.Ldc_I4,
        OpCodes.Box,
        OpCodes.Ldsfld,
        OpCodes.Call,
        OpCodes.Ret
    ];
    
    /// <summary>
    /// 2015 signature for obfuscated builds (works with both Eazfuscator and SA)
    /// </summary>
    private static OpCode[] _getVersionSignatureObf =
    [
        OpCodes.Ldc_I4,
        OpCodes.Call,
        OpCodes.Call,
        OpCodes.Ldsfld,
        OpCodes.Call,
        OpCodes.Ret
    ];
    
    static MethodInfo? _getVersionRef = AssemblyUtils.OsuTypes
        .Where(t => t.IsClass &&
                    t.IsAbstract &&
                    t.IsSealed &&
                    t.IsNotPublic)
        .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
        .FirstOrDefault(m => m.GetParameters().Length == 0 &&
                             m.ReturnType.FullName == "System.String" &&
                             SigScanning.CompareMultipleSigs(m, [
                                 _getVersionSignature, 
                                 _getVersionSignatureObf, 
                                 _getVersionSignatureOld, 
                                 _getVersionSignatureOldOld, 
                                 _getVersionSignatureOldOldObf
                             ])
        );
    
    public static string GetVersion() => _getVersionRef?.Invoke(null, null) as string;
}
