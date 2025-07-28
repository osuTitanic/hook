using System;
using System.IO;
using System.Security.Cryptography;

namespace TitanicHookManaged.Helpers;

public static class ChecksumUtils
{
    /// <summary>
    /// Calculate SHA256 sum of a file
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string CalculateSha256(string filename)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filename);
        byte[] hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// Calculate MD5 sum of a file
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static string CalculateMd5(string filename)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filename);
        byte[] hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }
}
