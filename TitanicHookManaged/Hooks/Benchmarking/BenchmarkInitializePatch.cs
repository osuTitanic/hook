using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Helpers.Benchmark;
using TitanicHookManaged.OsuInterop;

namespace TitanicHookManaged.Hooks.Benchmarking;

/// <summary>
/// One part of the benchmark submission patch - this one handles the initialization of the benchmark
/// </summary>
public class BenchmarkInitializePatch : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.BenchmarkInitialize";

    public BenchmarkInitializePatch() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Prefixes = [AccessTools.Method(typeof(BenchmarkInitializePatch), nameof(BenchmarkInitializePrefix))];
    }
    
    private static void BenchmarkInitializePrefix()
    {
        Logging.HookTrigger(HookName);
        
        // Try to see if we can get user credentials from the config
        ConfigReader osuCfg = new ();
        string username = osuCfg.TryGetValue("Username");
        string password = osuCfg.TryGetValue("Password");
        if (username == "" || password == "")
        {
            Notifications.ShowMessage("Couldn't get username and password. Make sure you log in with \"Save password\" option checked and restart the game. This benchmark score won't get submitted");
            return;
        }

        if (EntryPoint.Config?.BenchmarkConsent == BenchmarkDataConsent.NotAsked)
        {
            // Ask user if we can submit benchmark hardware details
            string messageBody =
                "Are you okay with submitting your hardware info for the benchmark test? This is NOT required and it is up to you if you want to enable this.\nThis is the information it will send:\n" +
                "- Renderer you selected in osu! (OpenGL or DirectX)\n" +
                "- CPU, Number of CPU Cores, Number of Logical Processors (threads)\n" +
                "- Your GPU name\n" +
                "- Your total RAM\n" +
                "- Your Operating System name\n" +
                "- Your OS Architecture (64-bit or 32-bit)\n" +
                "- Motherboard Manufacturer, Motherboard";
            DialogResult res = MessageBox.Show(messageBody, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (res == DialogResult.Yes)
                EntryPoint.Config.BenchmarkConsent = BenchmarkDataConsent.Allowed;
            else if (res == DialogResult.No)
                EntryPoint.Config.BenchmarkConsent = BenchmarkDataConsent.NotAllowed;
            
            EntryPoint.Config.SaveConfiguration(EntryPoint.Config.Filename);
        }
        
        Notifications.ShowMessage($"Hardware details will{(EntryPoint.Config.BenchmarkConsent == BenchmarkDataConsent.Allowed ? "" : " NOT")} be submitted");
        Notifications.ShowMessage("Make sure your FPS limiter is set to Unlimited to get better score!");
    }

    private static MethodInfo? GetTargetMethod()
    {
        Type? benchmarkType = AssemblyUtils.OsuTypes
            .FirstOrDefault(t => t.FullName == "osu.GameModes.Options.Benchmark"); // TODO: Newer builds obfuscate this symbol name
        
        if (benchmarkType == null)
        {
            Logging.HookError(HookName, "Failed to find benchmark type");
            return null;
        }
        
        MethodInfo? benchmarkInitializeMethod = benchmarkType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m => m.ReturnType.FullName == "System.Void" &&
                                 m.GetParameters().Length == 0 &&
                                 SigScanning.GetStrings(m)
                                     .Any(s => s.Contains("Please click to start the benchmark.\nWhile running do not move your mouse or switch windows."))
            );
        
        return benchmarkInitializeMethod;
    }
}
