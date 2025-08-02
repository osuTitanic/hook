#if NET20
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

    private static TcpOverWebsocket _conn;
    
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
        bool tcpClientCreationShouldNop = false;
        List<CodeInstruction> codes = new (instructions);
        //foreach (CodeInstruction instr in codes)
        for (int i = 0; i < codes.Count; i++)
        {
            CodeInstruction instr = codes[i];

            if (instr.opcode == OpCodes.Call && instr.operand is MethodInfo { Name: "disconnect" })
            {
                instr.opcode = OpCodes.Nop;
                continue;
            }
            
            // Replace the call to TcpClient creation with a dummy call
            if (instr.opcode == OpCodes.Call && instr.operand is MethodInfo method && IsClientConnectMethod(method))
            {
                Logging.HookStep(HookName, "Replacing client connect in transpiler");
                //instr.operand = AccessTools.Constructor(typeof(TcpOverWebsocket), [typeof(IPEndPoint), typeof(int)]);
                instr.operand = AccessTools.Method(typeof(TcpOverWebsocket), nameof(TcpOverWebsocket.FakeConnect), [typeof(IPEndPoint), typeof(int)]);
                // instr.operand = OpCodes.Nop;
                tcpClientCreationShouldNop = true; // Assigning the client to the field should be NOPed out on the next iteration of the loop
                _conn = new TcpOverWebsocket();

                CodeInstruction instr2 = codes[i + 1];
                instr2.opcode = OpCodes.Nop;
                continue;
            }

            // The next instruction after TcpClient creation is assigning it to a field. We NOP that instruction
            // if (tcpClientCreationShouldNop)
            // {
            //     Logging.HookStep(HookName, "Replacing field reference with NOP");
            //     instr.opcode = OpCodes.Nop;
            //     instr.operand = null;
            //     tcpClientCreationShouldNop = false;
            //     continue;
            // }

            // Here we have to replace the GetStream of BanchoClient's TcpClient with our own client
            if (instr.opcode == OpCodes.Callvirt && instr.operand is MethodInfo { Name: "GetStream" })
            {
                Logging.HookStep(HookName, "Replacing stream");
                instr.operand = AccessTools.Method(typeof(TcpOverWebsocket), nameof(TcpOverWebsocket.GetStream));
                
                // Override the static field instruction above with our own static field
                CodeInstruction instrAbove = codes[i - 1];
                instrAbove.operand = AccessTools.Field(typeof(WebSocketPatch), nameof(_conn));
                continue;
            }
        }
        return codes.AsEnumerable();
    }

    private static bool IsClientConnectMethod(MethodInfo m)
    {
        ParameterInfo[] prms = m.GetParameters();
        return prms.Length == 2 && 
               prms[0].ParameterType.FullName == "System.Net.IPEndPoint" &&
               prms[1].ParameterType.FullName == "System.Int32" &&
               m.ReturnType.FullName == "System.Net.Sockets.TcpClient" &&
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
