// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025-2026 Oreeeee

using System;
using System.Text.RegularExpressions;

namespace TitanicHook.Core.OsuInterop.Version;

public abstract class OsuVersionProvider
{
    public abstract string GetVersion();
    public abstract bool CanProvideVersion();

    public string GetProviderName()
    {
        return this.GetType().Name;
    }

    /// <summary>
    /// Gets version number (e.g. 20150101) from long one (e.g. b20150101cuttingedge)
    /// </summary>
    /// <returns></returns>
    public int GetVersionNumber()
    {
        if (_versionNumberCache != null)
            return _versionNumberCache.Value;

        string? fullVersion = GetVersion();
        if (string.IsNullOrEmpty(fullVersion))
            return 0;

        var match = Regex.Match(fullVersion, @"b(\d{8})"); // This regex only supports 8-digit numbers, might be adjusted if old build numbers are ever needed 
        if (!match.Success) return 0;

        string version = match.Groups[1].Value;
        _versionNumberCache = int.Parse(version);

        return _versionNumberCache.Value;
    }

    private int? _versionNumberCache = null;

    public static OsuVersionProvider GetDefaultVersionProvider()
    {
        FunctionCallVersionProvider functionProvider = new FunctionCallVersionProvider();
        if (functionProvider.CanProvideVersion())
            return functionProvider;

        BanchoVersionProvider banchoProvider = new BanchoVersionProvider();
        if (banchoProvider.CanProvideVersion())
            return banchoProvider;

        throw new Exception("Couldn't find an available version provider");
    }
}
