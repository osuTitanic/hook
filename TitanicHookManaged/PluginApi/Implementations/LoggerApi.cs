using TitanicHook.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.PluginApi.Implementations;

public class LoggerApi : ILogger
{
    public void HookStart(string hookName) => Logging.HookStart(hookName);
    public void HookDone(string hookName) => Logging.HookDone(hookName);
    public void HookStep(string hookName, string message) => Logging.HookStep(hookName, message);
    public void HookPatching(string hookName) => Logging.HookPatching(hookName);
    public void HookError(string hookName, string message, bool showError = true) => Logging.HookError(hookName, message, showError);
    public void HookTrigger(string hookName) => Logging.HookTrigger(hookName);
    public void HookOutput(string hookName, string message) => Logging.HookOutput(hookName, message);
    public void Info(string message) => Logging.Info(message);
    public void ShowError(string message) => Logging.ShowError(message);
    public void LogAndShowError(string message) => Logging.LogAndShowError(message);
}
