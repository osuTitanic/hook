using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
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

        if (loaded.ImageRuntimeVersion != Assembly.GetExecutingAssembly().ImageRuntimeVersion)
        {
            MessageBox.Show(".NET Framework runtime version mismatch! You have to use a different version of TitanicHookManaged.", "Mismatch!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        
        // Get entry point
        MethodInfo entry = loaded.EntryPoint;
        if (entry == null)
        {
            Console.WriteLine("Entry point not found.");
            return;
        }
        
        List<string> FakeArgs = new();
        FakeArgs.Add(path);
#if NET40
        FakeArgs.Add("-go");
#endif
        
        // Load hooks specific to the loader
        EntryPointHook.Initialize(loaded);
        ExecutablePathHook.Initialize(path);
        ExtractIconHook.Initialize();
        GetArgsHook.Initialize(FakeArgs.ToArray());
        
        // Hook osu!.exe's entrypoint to execute other hooks there
        // This is required because osu!common has to be loaded by osu! for hooking
        // If we would've loaded osu!common manually it wouldn't work
        OsuStartHook.Initialize(entry);
        
        // Start the exe's entry point
        entry.Invoke(null, new object[] { });
    }
}
