// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025-2026 Oreeeee

using System;

namespace TitanicHook.Core.OsuInterop.Version;

/// <summary>
/// Provides osu! version by manually setting the osu! version from a reliable source, like the launcher
/// </summary>
public class StaticVersionProvider : OsuVersionProvider
{
    private string? _version = null;

    public StaticVersionProvider(int version)
    {
        _version = "b" + version;
    }

    public override bool CanProvideVersion()
    {
        return _version != null;
    }

    public override string GetVersion()
    {
        if (!CanProvideVersion())
            throw new InvalidOperationException("This version provider is unable to provide the version");

        return _version;
    }
}
