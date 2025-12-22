using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.Helpers.Benchmark;
using TitanicHookManaged.OsuInterop;

namespace TitanicHookManaged.Hooks.Benchmarking;

/// <summary>
/// Second part of the benchmark patch - this one handles stage updates and submission
/// </summary>
public class BenchmarkStagePatch : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.BenchmarkStage";
    
    public BenchmarkStagePatch() : base(HookName)
    {
        TargetMethods = [GetTargetMethod()];
        Transpilers = [AccessTools.Method(typeof(BenchmarkStagePatch), nameof(BenchmarkTranspiler))];
    }
    
    private static IEnumerable<CodeInstruction> BenchmarkTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        CodeInstruction[] input = instructions.ToArray();
        List<CodeInstruction> output = new ();
        List<OpCode> smoothnessOpCodes = new ();
        bool insertBenchmarkCallbackAfterCallvirt = false;
        for (int i = 0; i < input.Length; i++)
        {
            CodeInstruction instr = input[i];
            
            output.Add(instr);
            
            // Deobfuscate string if obfuscated
            string? deobfuscatedString = "";
            if (ObfHelper.HasStringDecrypt && instr.opcode == OpCodes.Call && instr.operand is MethodInfo methodInfo &&
                methodInfo.MetadataToken == ObfHelper.StringObfToken)
            {
                var stringIdInstr = input[i - 1];
                int stringId = (int)stringIdInstr.operand;

                deobfuscatedString = ObfHelper.DecString(stringId);
            }
            
            if (instr.opcode == OpCodes.Ldstr && instr.operand is string str && str == "\nRaw Score: " || deobfuscatedString == "\nRaw Score: ")
            {
                insertBenchmarkCallbackAfterCallvirt = true;
            }
            
            if (instr.opcode == OpCodes.Ldstr && instr.operand is string str2 && str2 == "\n\nOverall Smoothness: " || deobfuscatedString == "\n\nOverall Smoothness: ")
            {
                // Get the opcodes for locals required to calculate the smoothness
                smoothnessOpCodes.Add(input[i + 1].opcode);
                smoothnessOpCodes.Add(input[i + 2].opcode);
            }

            if (instr.opcode == OpCodes.Callvirt && insertBenchmarkCallbackAfterCallvirt)
            {
                // Inject the benchmark finished callback here
                insertBenchmarkCallbackAfterCallvirt = false;
                MethodInfo a = AccessTools.Method(typeof(BenchmarkStagePatch), nameof(BenchmarkCallback));
                Logging.HookStep(HookName, $"a={a.Name}");
                output.Add(new CodeInstruction(OpCodes.Ldarg_0)); // Load the class instance
                output.Add(new CodeInstruction(smoothnessOpCodes[0])); // Load cumulative score
                output.Add(new CodeInstruction(smoothnessOpCodes[1])); // Load testRun
                output.Add(new CodeInstruction(OpCodes.Div)); // Divide these to an int32
                output.Add(new CodeInstruction(OpCodes.Call, a)); // Call the callback
            }
        }
        
        return output.AsEnumerable();
    }

    private static void BenchmarkCallback(object benchmark, int smoothness)
    {
        Logging.Info($"Called benchmark callback with object of type {benchmark.GetType().FullName}");
        
        // Get the dictionary field
        FieldInfo? field = benchmark.GetType()
            .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(f => f.FieldType.IsGenericType &&
                                 f.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
                                 f.FieldType.GetGenericArguments()[0].IsEnum &&
                                 f.FieldType.GetGenericArguments()[1] == typeof(int)
                                 );

        if (field == null)
        {
            Logging.Info("Couldn't find benchmark scores field type");
            return;
        }
        
        // Get scores field (as object)
        object dictObj = field.GetValue(benchmark);

        // Convert to a dictionary that can be used here
        Dictionary<string, int> scores = new();

        foreach (var kv in (IEnumerable)dictObj)
        {
            var keyProp = kv.GetType().GetProperty("Key")!;
            var valProp = kv.GetType().GetProperty("Value")!;

            object key = keyProp.GetValue(kv, null)!;
            object val = valProp.GetValue(kv, null)!;
            
            scores[key.ToString()!] = (int)val;
        }
        
        Logging.Info("Benchmark scores:");
        foreach (var kv in scores)
        {
            Logging.Info($"{kv.Key}: {kv.Value}");
        }
        Logging.Info($"Overall Smoothness: {smoothness}");

        int rawScore = 0;
        foreach (var value in scores.Values)
        {
            rawScore += value;
        }
        int idleFramerate = scores["IdleFramerate"] / 6; // why specifically 6?

        Logging.Info($"Raw Score: {rawScore}");
        Logging.Info($"Idle framerate (divided): {idleFramerate}");

        bool submitFullHardware = EntryPoint.Config?.BenchmarkConsent == BenchmarkDataConsent.Allowed;
        Hardware hw = HardwareDetection.GetHardwareInfo(submitFullHardware);
        Logging.Info($"Renderer: {hw.renderer}");
        if (submitFullHardware)
        {
            Logging.Info($"CPU: {hw.cpu}");
            Logging.Info($"Cores: {hw.cores}");
            Logging.Info($"Threads: {hw.threads}");
            Logging.Info($"GPU: {hw.gpu}");
            Logging.Info($"RAM: {hw.ram}");
            Logging.Info($"OS: {hw.osInfo}");
            Logging.Info($"OS Architecture: {hw.osArchitecture}");
            Logging.Info($"Motherboard manufacturer: {hw.motherboardManufacturer}");
            Logging.Info($"Motherboard: {hw.motherboard}");
        }
        
        // Do some pre-submit checks
        string osuVersion = OsuVersion.GetVersion();
        if (string.IsNullOrEmpty(osuVersion))
        {
            Notifications.ShowMessage("Couldn't get the osu! version. This score won't get submitted");
            return;
        }
        
#if NET20
        osuVersion += " (.NET 2.0)";
#elif NET40
        osuVersion += " (.NET 4.0)";
#endif
        
        // Submit the score
        string hardwareInfo = submitFullHardware
            ? $"{{\"renderer\":\"{hw.renderer}\", \"cpu\":\"{hw.cpu}\", \"cores\":{hw.cores}, \"threads\":{hw.threads}, " +
              $"\"gpu\":\"{hw.gpu}\", \"ram\":{hw.ram}, \"os\":\"{hw.osInfo} ({hw.osArchitecture})\", " +
              $"\"motherboard_manufacturer\":\"{hw.motherboardManufacturer}\", \"motherboard\":\"{hw.motherboard}\"}}" 
            : $"{{\"renderer\":\"{hw.renderer}\"}}";
        
        ConfigReader osuCfg = new ();
        NameValueCollection scoreData = new()
        {
            {"u", osuCfg.TryGetValue("Username")},
            {"p", osuCfg.TryGetValue("Password")},
            {"s", smoothness.ToString()},
            {"f", idleFramerate.ToString()},
            {"r", rawScore.ToString()},
            {"c", osuVersion},
            {"h", hardwareInfo},
        };
        using (var wc = new WebClient())
        {
            byte[] result;
            try
            {
                result = wc.UploadValues($"http://osu.{EntryPoint.Config?.ServerName}/web/osu-benchmark.php",
                    scoreData);
            }
            catch (Exception e)
            {
                Notifications.ShowMessage($"Failed to submit benchmark score: {e.Message}");
                return;
            }
            Notifications.ShowMessage("Successfully submitted benchmark score");
        }
        string message = !submitFullHardware 
            ? $"Benchmark Results:\n" +
           $"Raw score: {rawScore}\n" +
           $"Framerate: {idleFramerate}fps\n" +
           $"Smoothness: {smoothness}%\n\n" +
           $"Renderer: {hw.renderer}\n"
           : $"Benchmark Results:\n" +
             $"Raw score: {rawScore}\n" +
             $"Framerate: {idleFramerate}fps\n" +
             $"Smoothness: {smoothness}%\n\n" +
             $"Renderer: {hw.renderer}\n" +
             $"CPU: {hw.cpu}\n" +
             $"Number of Cores: {hw.cores}\n" +
             $"Logical Processors (Threads): {hw.threads}\n" +
             $"Graphics Card: {hw.gpu}\n" +
             $"Total Ram: {hw.ram} GB\n" +
             $"Operating System: {hw.osInfo} ({hw.osArchitecture})\n" +
             $"Motherboard Manufacturer: {hw.motherboardManufacturer}\n" +
             $"Motherboard: {hw.motherboard}";
        MessageBox.Show(message);
        return;
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
        
        MethodInfo? benchmarkStageMethod = benchmarkType
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.ReturnType.FullName == "System.Void" &&
                                 m.GetParameters().Length == 0 &&
                                 SigScanning.GetStrings(m)
                                     .Any(s => s.Contains("Running stage {0} of {1}\n{2}"))
            );
        
        return benchmarkStageMethod;
    }
}