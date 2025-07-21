using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace HookLoader;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "osu!.exe");
        string commonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "osu!common.dll");
        
        // Load osu!
        Assembly? loaded = Assembly.Load(File.ReadAllBytes(path));

        // Load osu!common too if it exists
        // TODO: BROKEN
        if (File.Exists(commonPath))
        {
            Assembly.Load(File.ReadAllBytes(commonPath));
        }
        
        MethodInfo entry = loaded.EntryPoint;
        if (entry == null)
        {
            Console.WriteLine("Entry point not found.");
            return;
        }
        
        // Load TitanicHook
        TitanicHookManaged.EntryPoint.Start("");
        
        EntryPointHook.Initialize(loaded);
        ExecutablePathHook.Initialize(path);
        ExtractIconHook.Initialize();
        
        entry.Invoke(null, new object[] { });
    }
}
