namespace TitanicHook.Framework.OsuInterop;

/// <summary>
/// An interface for fetching the current osu! version
/// </summary>
public interface IOsuVersions
{
    /// <summary>
    /// Gets the full version as a string
    /// </summary>
    /// <returns>Full osu! version (for example b20150101cuttingedge), null if unable to get</returns>
    public string? GetVersion();

    /// <summary>
    /// Gets the short osu! version
    /// </summary>
    /// <returns>Short osu! version (for example 20150101), 0 if unable to get</returns>
    public int GetVersionNumber();
}
