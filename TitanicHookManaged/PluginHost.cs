using TitanicHook.Framework;
using TitanicHook.Framework.OsuInterop;
using TitanicHookManaged.OsuInterop;

namespace TitanicHookManaged;

public class PluginHost : IPluginHost
{
    public INotificationManager NotificationManager { get; } = new Notifications();
    public IOsuVersions OsuVersions { get; } = new OsuVersion();
}
