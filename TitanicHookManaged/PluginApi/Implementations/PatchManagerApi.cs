using TitanicHook.Framework;
using TitanicHookManaged.Framework;

namespace TitanicHookManaged.PluginApi.Implementations;

public class PatchManagerApi : IPatchManager
{
    public bool Apply(object foo)
    {
        // TODO: Update to actually take TitanicPatch once that gets added to the plugin API
        return PatchManager.Apply((TitanicPatch)foo);
    }
}
