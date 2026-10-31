// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;

namespace TitanicHook.Core.Helpers;

public static class OsuFileUtils
{
    static string[] OsuExecutableNames = {
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
        if (string.IsNullOrEmpty(directory))
            return null;

        foreach (string file in OsuExecutableNames)
        {
            string path = Path.Combine(directory, file);

            if (File.Exists(path))
                return path;
        }

        return null;
    }
}
