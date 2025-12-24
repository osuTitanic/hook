using TitanicHook.Framework.OsuInterop;
using TitanicHookManaged.OsuInterop;

namespace TitanicHookManaged.PluginApi.Implementations;

public class OsuVersionApi : IOsuVersions
{
    public string? GetVersion() => OsuVersion.GetVersion();

    public int GetVersionNumber() => OsuVersion.GetVersionNumber();
}
