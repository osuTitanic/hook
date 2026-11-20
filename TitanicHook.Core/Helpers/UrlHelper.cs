using System;
using System.Collections.Generic;
using System.Text;

namespace TitanicHook.Core.Helpers;

public class UrlHelper
{
    /// <summary>
    /// Returns an url prefixed with http:// or https:// depending on whether TLS 1.2 with Elliptic Curve encryption is supported or not
    /// </summary>
    /// <param name="url">Original url</param>
    /// <returns>Url with prefix</returns>
    public static string GetUrl(string url)
    {
        string protocol = IsTlsSupported ? "https" : "http";
        return $"{protocol}://{url}";
    }

    public static Uri GetUri(string uri)
    {
        return new Uri(GetUrl(uri));
    }

    private static bool? _backingTlsSupported;
    public static bool IsTlsSupported
    {
        get
        {
            if (_backingTlsSupported != null)
                return (bool)_backingTlsSupported;
            
            if (WinApi.RunningUnderWine)
            {
                // Wine might misreport some stuff, however it does support TLS
                _backingTlsSupported = true;
                return (bool)_backingTlsSupported;
            }

            OperatingSystem os = Environment.OSVersion;
            int osBuild = os.Version.Build;

            if (osBuild > 7601)
            {
                // Everything after Windows 7 has TLS 1.2
                _backingTlsSupported = true;
                return (bool)_backingTlsSupported;
            }

            if (osBuild == 7601 && WinApi.IsWinUpdateInstalled("KB3140245"))
            {
                // Windows 7 with KB3140245 supports TLS 1.2
                _backingTlsSupported = true;
                return (bool)_backingTlsSupported;
            }

            // TLS 1.2 is unsupported
            _backingTlsSupported = false;
            return (bool)_backingTlsSupported;
        }
    }
}
