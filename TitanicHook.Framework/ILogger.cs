namespace TitanicHook.Framework;

/// <summary>
/// Allows to log stuff to the console/file.
/// This logger is shared with the main Hook logger
/// </summary>
public interface ILogger
{
    public void HookStart(string hookName);
    public void HookDone(string hookName);
    public void HookStep(string hookName, string message);
    public void HookPatching(string hookName);
    public void HookError(string hookName, string message, bool showError = true);
    public void HookTrigger(string hookName);
    public void HookOutput(string hookName, string message);
    public void Info(string message);
    public void ShowError(string message);
    public void LogAndShowError(string message);
}
