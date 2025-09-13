// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.OsuInterop;

// Contains stuff to get osu! version through BanchoClient in older builds

public partial class OsuVersion
{
    /// <summary>
    /// Reference to BanchoClient Connect
    /// </summary>
    private static MethodInfo? _banchoConnect = AssemblyUtils.OsuTypes
        .FirstOrDefault(t => t.IsClass &&
                             t.IsNotPublic &&
                             t.IsSealed &&
                             t.IsAbstract &&
                             (t.BaseType == typeof(object) || t.BaseType == null) &&
                             t.GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                                 .Any(f => f.FieldType == typeof(TcpClient)))
        .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
        .FirstOrDefault(m => m.GetParameters().Length == 0 &&
                             m.ReturnType.FullName == "System.Void" &&
                             (SigScanning.GetStrings(m).Any(s => s.StartsWith("{0}|{1}|{2}|{3}")) || SigScanning.GetStrings(m).Contains("Connecting to Bancho...")));

    /// <summary>
    /// Looks for osu! version by BanchoClient Connect IL instructions
    /// </summary>
    private static string? GetOsuVersionFromBancho()
    {
        if (_banchoConnect == null)
            return null;
        
        var reader = new ILReader(_banchoConnect);
        ILInstruction[] instructions = reader.ToArray();
        bool nextStringRefIsOsuVersion = false;
        for (int i = 0; i < instructions.Length; i++)
        {
            ILInstruction instr = instructions[i];
            if (instr.OpCode == OpCodes.Ldstr && instr is InlineStringInstruction instrStr)
            {
                if (instrStr.String.StartsWith("{0}|{1}|{2}|{3}"))
                {
                    nextStringRefIsOsuVersion = true;
                    continue;
                }
            }

            if (instr.OpCode == OpCodes.Call && instr is InlineMethodInstruction callInstr &&
                callInstr.Token == ObfHelper.StringObfToken)
            {
                // Get argument for string decrypt method
                if (instructions[i - 1] is not InlineIInstruction loadArg) continue;

                int arg = loadArg.Int32;
                string? str = ObfHelper.DecString(arg);
                
                if (str != null && str.StartsWith("{0}|{1}|{2}|{3}"))
                {
                    nextStringRefIsOsuVersion = true;
                    continue;
                }
            }

            if (instr.OpCode == OpCodes.Ldsfld && instr is InlineFieldInstruction fieldInstr &&
                nextStringRefIsOsuVersion)
            {
                if (fieldInstr.Field.FieldType.FullName != "System.String")
                    continue;
                
                object value = fieldInstr.Field.GetValue(null);
                return value as string;
            }
        }

        return null;
    }
}
