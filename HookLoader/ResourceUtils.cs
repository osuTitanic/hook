using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace HookLoader;

public static class ResourceUtils
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
    /// Read bytes of embedded resource
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    public static byte[]? GetEmbeddedResource(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(filename);
        if (stream == null)
        {
            return null;
        }
            
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }
}
