// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.IO;

namespace TitanicHook.Core.Helpers;

public static class OsuFileUtils
{
    private static readonly string[] OsuExecutableNames = {
        "osu!.exe",
        "osu.exe",
        "osu!test.exe",
        "osu!public.exe",
        "osu!shine1.exe",
        "osu!cuttingedge.exe"
    };

    /// <summary>
    /// Tries to find the osu! executable in a directory
    /// </summary>
    /// <param name="directory">Directory to search in</param>
    /// <returns>Path to a first found osu! executable, null if none was found</returns>
    public static string? FindOsuExecutable(string? directory)
    {
        foreach (string path in FindOsuExecutables(directory))
            return path;

        return null;
    }

    /// <summary>
    /// Tries to find all osu! executables in a directory
    /// </summary>
    /// <param name="directory">Directory to search in</param>
    /// <returns>Enumerable of all found osu! executable paths</returns>
    public static IEnumerable<string> FindOsuExecutables(string? directory)
    {
        if (string.IsNullOrEmpty(directory))
            yield break;

        foreach (string file in OsuExecutableNames)
        {
            string path = Path.Combine(directory, file);

            if (File.Exists(path))
                yield return path;
        }
    }
}
