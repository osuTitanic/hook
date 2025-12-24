namespace TitanicHook.Framework;

/// <summary>
/// Exposes the patch manager that's used for applying patches
/// </summary>
public interface IPatchManager
{
    public bool Apply(object foo);
}
