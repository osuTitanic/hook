using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks;

/// <summary>
/// Patch that will allow for submitting scores from in-game benchmark
/// </summary>
public static class BenchmarkSubmitPatch
{
    public const string HookName = "sh.Titanic.Hook.BenchmarkSubmit";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);

        MethodInfo? targetMethod = AssemblyUtils.OsuTypes
            .Where(t => t.FullName == "osu.GameModes.Options.Benchmark") // TODO: Newer builds obfuscate this symbol name
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            .FirstOrDefault(m => m.ReturnType.FullName == "System.Void" &&
                                 m.GetParameters().Length == 0 &&
                                 SigScanning.GetStrings(m)
                                     .Any(s => s.Contains("Running stage {0} of {1}\n{2}"))
            );

        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Failed to find benchmark");
            return;
        }

        var transpiler = typeof(BenchmarkSubmitPatch).GetMethod("BenchmarkTranspiler", Constants.HookBindingFlags);

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(targetMethod, transpiler: new HarmonyMethod(transpiler));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
        }
        
        Logging.HookDone(HookName);
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
            if (instr.opcode == OpCodes.Ldstr && instr.operand is string str && str == "\nRaw Score: ")
            {
                insertBenchmarkCallbackAfterCallvirt = true;
            }
            
            if (instr.opcode == OpCodes.Ldstr && instr.operand is string str2 && str2 == "\n\nOverall Smoothness: ")
            {
                // Get the opcodes for locals required to calculate the smoothness
                smoothnessOpCodes.Add(input[i + 1].opcode);
                smoothnessOpCodes.Add(input[i + 2].opcode);
            }

            if (instr.opcode == OpCodes.Callvirt && insertBenchmarkCallbackAfterCallvirt)
            {
                // Inject the benchmark finished callback here
                insertBenchmarkCallbackAfterCallvirt = false;
                MethodInfo a = AccessTools.Method(typeof(BenchmarkSubmitPatch), nameof(BenchmarkCallback));
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
    }
}
