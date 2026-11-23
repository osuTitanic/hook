// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025-2026 Oreeeee

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TitanicHook.Core.Helpers;

namespace TitanicHook.Core.OsuInterop.Version;

/// <summary>
/// Obtains the osu! version through calling a function inside osu! that returns the version
/// </summary>
public class FunctionCallVersionProvider : OsuVersionProvider
{
    public override bool CanProvideVersion()
    {
        return _getVersionRef != null;
    }

    public override string GetVersion()
    {
        if (!CanProvideVersion())
            throw new InvalidOperationException("This version provider is unable to provide the version");

        if (_versionCache != null)
            return _versionCache;

        _versionCache = _getVersionRef?.Invoke(null, null) as string ?? "";
        return _versionCache;
    }

    private string? _versionCache = null;

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

    private MethodInfo? _getVersionRef = AssemblyUtils.OsuTypes
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
}
