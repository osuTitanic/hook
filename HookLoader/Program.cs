using System;
using System.IO;
using System.Reflection;
using TitanicHookManaged.Hooks.Managed;

namespace HookLoader;

class Program
{
    /// <summary>
    /// Small program that will load osu!.exe into memory, execute hooks, and start osu!'s main function
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    static void Main(string[] args)
    {
        // Load osu!
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "osu!.exe");
        Assembly loaded = Assembly.Load(File.ReadAllBytes(path));
        
        // Get entry point
        MethodInfo entry = loaded.EntryPoint;
        if (entry == null)
        {
            Console.WriteLine("Entry point not found.");
            return;
        }
        
        // Load hooks specific to the loader
        EntryPointHook.Initialize(loaded);
        ExecutablePathHook.Initialize(path);
        ExtractIconHook.Initialize();
        GetArgsHook.Initialize([path]);
        
        // Hook osu!.exe's entrypoint to execute other hooks there
        // This is required because osu!common has to be loaded by osu! for hooking
        // If we would've loaded osu!common manually it wouldn't work
        OsuStartHook.Initialize(entry);
        
        // Start the exe's entry point
        entry.Invoke(null, new object[] { });
    }
}
