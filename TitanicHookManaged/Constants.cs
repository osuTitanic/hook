using System.Reflection;

namespace TitanicHookManaged;

public static class Constants
{
    /// <summary>
    /// Binding flags for reflecting managed hooks
    /// </summary>
    public const BindingFlags HookBindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

    public const string DefaultConfigName = "TitanicHook.cfg";
}
