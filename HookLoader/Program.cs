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
        
        // Load osu!
        byte[] assemblyBytes = File.ReadAllBytes(path);
        Assembly? loaded = Assembly.Load(assemblyBytes);
        
        MethodInfo entry = loaded.EntryPoint;
        if (entry == null)
        {
            Console.WriteLine("Entry point not found.");
            return;
        }
        
        // Load TitanicHook
        TitanicHookManaged.EntryPoint.Start("");
        
        EntryPointHook.Initialize(loaded);
        
        entry.Invoke(null, new object[] { });
    }
}
