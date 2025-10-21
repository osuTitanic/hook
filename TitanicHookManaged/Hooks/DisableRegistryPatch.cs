using System;
using System.Linq;
using System.Reflection;
using Harmony;
using Harmony.ILCopying;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

public static class DisableRegistryPatch
{
    public const string HookName = "sh.Titanic.Hook.DisableRegistryPatch";

    public static void Initialize()
    {
        Logging.HookStart(HookName);
        var harmony = HarmonyInstance.Create(HookName);

        MethodInfo? targetMethod = AssemblyUtils.OsuTypes
            .Where(t => t is { IsClass: true, IsSealed: true, IsNested: false })
            .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            .FirstOrDefault(m => SigScanning.GetStrings(m)
                                    .Any(s => s == "osu! beatmap v2"));

        if (targetMethod == null)
        {
            // It's not really a problem if this can't be found, since it most likely means the client is too old
            //Logging.HookError(HookName, "Failed to find GameBase HandleAssociations");
            return;
        }
        
        var prefix = AccessTools.Method(typeof(DisableRegistryPatch), nameof(StubMethod));
        
        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, new HarmonyMethod(prefix));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
    }
    
    private static bool StubMethod() => false;
}
