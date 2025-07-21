using System.Diagnostics;
using HoLLy.ManagedInjector;

namespace TestInjector;

class Program
{
    static void Main(string[] args)
    {
        Console.Write("TargetFramework (net20 if empty): ");
        string targetFramework = Console.ReadLine();
        
        Console.Write("Configuration (Debug if empty): ");
        string configuration = Console.ReadLine();
        
        Console.Write("Process name without exe (osu! if empty): ");
        string processName = Console.ReadLine();
        
        if (targetFramework == "")
            targetFramework = "net20";
        
        if (configuration == "")
            configuration = "Debug";
        
        if (processName == "")
            processName = "osu!";

        Process? osuProcess = null;
        foreach (Process process in Process.GetProcesses())
        {
            if (process.ProcessName == processName)
            {
                osuProcess = process;
            }
        }

        if (osuProcess == null)
        {
            Console.WriteLine($"Couldn't find {processName}");
            return;
        }
        Console.WriteLine($"PID of {processName}: {osuProcess.Id}");
        
        string injecteePath = Path.Combine(Environment.CurrentDirectory, $"TitanicHookManaged/bin/{configuration}/{targetFramework}/TitanicHookManaged.dll");
        Console.WriteLine($"Injectee path: {injecteePath}");
        
        InjectableProcess injectableProcess = new InjectableProcess((uint)osuProcess.Id);
        injectableProcess.Inject(injecteePath, "TitanicHookManaged.EntryPoint", "InjectedTarget");
        
        Console.WriteLine("Injected");
    }
}
