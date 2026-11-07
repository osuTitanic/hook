using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TitanicHook.GUI
{
    internal static class Runtime
    {
        public static bool UsesFallback(string executable)
        {
            StringBuilder version = new StringBuilder(256);

            int length;
            int error = GetFileVersion(executable, version, version.Capacity, out length);

            // Convert error code to exception and throw if it failed
            Marshal.ThrowExceptionForHR(error);

            string runtime = version.ToString();
            if (runtime.StartsWith("v2.", StringComparison.Ordinal)) return true;
            if (runtime.StartsWith("v4.", StringComparison.Ordinal)) return false;

            throw new InvalidDataException("Unsupported client runtime: " + runtime);
        }

        // https://learn.microsoft.com/en-us/dotnet/framework/unmanaged-api/hosting/getfileversion-function
        [DllImport("mscoree.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int GetFileVersion(string filename, StringBuilder buffer, int capacity, out int length);
    }
}
