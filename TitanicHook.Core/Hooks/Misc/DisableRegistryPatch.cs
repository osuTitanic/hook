using System.Linq;
using System.Reflection;
using Harmony;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Helpers;

namespace TitanicHook.Core.Hooks.Misc;

public class DisableRegistryPatch : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.DisableRegistryPatch";

    public DisableRegistryPatch() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Prefixes = [AccessTools.Method(typeof(DisableRegistryPatch), nameof(StubMethod))];
    }

    private static MethodInfo GetTargetMethod()
    {
        return AssemblyUtils.OsuTypes
            .Where(t => t is { IsClass: true, IsSealed: true, IsNested: false })
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            .FirstOrDefault(m => SigScanning.GetStrings(m)
                .Any(s => s == "osu! beatmap v2"));
    }
    
    private static bool StubMethod() => false;
}
