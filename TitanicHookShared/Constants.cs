using System.Reflection;

namespace TitanicHookShared;

public static class Constants
{
    /// <summary>
    /// Binding flags for reflecting managed hooks
    /// </summary>
    public const BindingFlags HookBindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

    public const string DefaultConfigName = "TitanicHook.cfg";
    public const string LogFileName = "TitanicHook.log";
}
