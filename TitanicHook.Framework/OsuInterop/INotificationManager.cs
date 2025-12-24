namespace TitanicHook.Framework.OsuInterop;

/// <summary>
/// Interface for osu!'s notification system
/// </summary>
public interface INotificationManager
{
    public void ShowMessage(string message);
}
