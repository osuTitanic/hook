// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Reflection;
using System.Reflection.Emit;
using TitanicHookManaged.Helpers;
using System.Linq;
using System.Text.RegularExpressions;

namespace TitanicHookManaged.OsuInterop;

public static partial class OsuVersion
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

    private static string? _osuVersionCache = null;

    /// <summary>
    /// Gets full name of the build (e.g. b20150101cuttingedge)
    /// </summary>
    /// <returns></returns>
    public static string? GetVersion()
    {
        if (_osuVersionCache == "")
            return null;
        
        if (_osuVersionCache != null)
            return _osuVersionCache;

        if (_getVersionRef != null)
        {
            _osuVersionCache = _getVersionRef.Invoke(null, null) as string;
        }
        else
        {
            _osuVersionCache = GetOsuVersionFromBancho();
        }
        return _osuVersionCache;
    }

    /// <summary>
    /// Gets version number (e.g. 20150101) from long one (e.g. b20150101cuttingedge)
    /// </summary>
    /// <returns></returns>
    public static int GetVersionNumber()
    {
        string? fullVersion = GetVersion();
        if (string.IsNullOrEmpty(fullVersion))
            return 0;
        
        var match = Regex.Match(fullVersion, @"b(\d{8})"); // This regex only supports 8-digit numbers, might be adjusted if old build numbers are ever needed 
        if (!match.Success) return 0;
        
        string version = match.Groups[1].Value;
        return int.Parse(version);
    }
}
