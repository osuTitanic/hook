#if NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using TitanicHookManaged.Helpers;
using TitanicHookManaged.WebSocketCompat;

namespace TitanicHookManaged.Hooks.Managed;

public static class WebSocketPatch
{
    public const string HookName = "sh.Titanic.Hook.WebSocketPatch";
    
    public static void Initialize()
    {
        Logging.HookStart(HookName);
        
        var harmony = HarmonyInstance.Create(HookName);
        
        // Try to get BanchoClient type
        Type? banchoClient = AssemblyUtils.OsuTypes
            .FirstOrDefault(t => t.IsClass &&
                                 t.IsNotPublic &&
                                 t.IsSealed &&
                                 t.IsAbstract &&
                                 (t.BaseType == typeof(object) || t.BaseType == null) &&
                                 t.GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                                     .Any(f => f.FieldType == typeof(TcpClient)));

        if (banchoClient == null)
        {
            Logging.HookError(HookName, "Couldn't find BanchoClient");
            return;
        }
        
        // Try to get the Connect method
        MethodInfo? banchoConnect = banchoClient
            .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.GetParameters().Length == 0 &&
                                 m.ReturnType.FullName == "System.Void" &&
                                 (SigScanning.GetStrings(m).Contains("{0}|{1}|{2}|{3}|{4}") || SigScanning.GetStrings(m).Contains("Connecting to Bancho...")));

        if (banchoConnect == null)
        {
            Logging.HookError(HookName, "Couldn't find Connect method");
            return;
        }
        
        var transpiler = typeof(WebSocketPatch).GetMethod("Transpiler", Constants.HookBindingFlags);

        try
        {
            Logging.HookPatching(HookName);
            harmony.Patch(banchoConnect, null, null, new HarmonyMethod(transpiler));
        }
        catch (Exception e)
        {
            Logging.HookError(HookName, e.ToString());
            return;
        }
        Logging.HookDone(HookName);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        //return instructions;
        List<CodeInstruction> codes = new (instructions);
        foreach (CodeInstruction instr in codes)
        {
            if (instr.opcode == OpCodes.Call && instr.operand is MethodInfo method && IsClientConnectMethod(method))
            {
                Logging.HookStep(HookName, "Replacing client connect in transpiler");
                instr.opcode = OpCodes.Newobj;
                instr.operand = AccessTools.Constructor(typeof(TcpOverWebsocket), [typeof(IPEndPoint), typeof(int)]);
            }
        }
        return codes.AsEnumerable();
    }

    private static bool IsClientConnectMethod(MethodInfo m)
    {
        ParameterInfo[] prms = m.GetParameters();
        return prms.Length == 2 && 
               prms[0].ParameterType.FullName == "System.Net.IPEndPoint" &&
               prms[1].ParameterType.FullName == "System.Net.Sockets.Socket" &&
               SigScanning.GetOpcodes(m).StartsWith(ClientConnectSignature);
    }

    private static readonly OpCode[] ClientConnectSignature =
    [
        OpCodes.Ldsfld,
        OpCodes.Callvirt,
        OpCodes.Pop,
        OpCodes.Ldnull,
        OpCodes.Stsfld,
        OpCodes.Ldarg_0,
        OpCodes.Callvirt,
        OpCodes.Call,
        OpCodes.Stloc_0,
        OpCodes.Ldarg_0,
        OpCodes.Callvirt,
        OpCodes.Stloc_1,
        OpCodes.Newobj,
        OpCodes.Stloc_2,
        OpCodes.Ldloc_2,
        OpCodes.Ldloc_0,
        OpCodes.Ldloc_1,
        OpCodes.Ldnull
    ];
}
#endif
