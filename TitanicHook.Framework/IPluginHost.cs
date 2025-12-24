using TitanicHook.Framework.OsuInterop;

namespace TitanicHook.Framework;

/// <summary>
/// Interface that provides access to Core's functionality
/// </summary>
public interface IPluginHost
{
    INotificationManager NotificationManager { get; }
    IOsuVersions OsuVersions { get; }
    ILogger Logger { get; }
}
