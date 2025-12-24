using TitanicHook.Framework.OsuInterop;
using TitanicHookManaged.OsuInterop;

namespace TitanicHookManaged.PluginApi.Implementations;

public class NotificationsApi : INotificationManager
{
    public void ShowMessage(string message) => Notifications.ShowMessage(message);
}
