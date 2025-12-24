namespace TitanicHook.Framework;

/// <summary>
/// Base interface for plugins
/// </summary>
public interface IPlugin
{
    public string Name { get; }
    public string Description { get; }
    public string Author { get; }
    public string Version { get; }
    public string License { get; }

    public void Initialize(IPluginHost host);
}
