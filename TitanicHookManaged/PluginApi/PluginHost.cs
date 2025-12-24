using TitanicHook.Framework;
using TitanicHook.Framework.OsuInterop;
using TitanicHookManaged.PluginApi.Implementations;

namespace TitanicHookManaged.PluginApi;

public class PluginHost : IPluginHost
{
    public INotificationManager NotificationManager { get; } = new NotificationsApi();
    public IOsuVersions OsuVersions { get; } = new OsuVersionApi();
}
