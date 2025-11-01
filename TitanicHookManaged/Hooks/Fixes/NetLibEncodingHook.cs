// SPDX-License-Identifier: GPL-3.0-or-later
// SPDX-FileCopyrightText: 2025 Oreeeee

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using ClrTest.Reflection;
using Harmony;
using TitanicHookManaged.Framework;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Hooks.Fixes;

public class NetLibEncodingHook : TitanicPatch
{
    public const string HookName = "sh.Titanic.Hook.NetLibEncoding";

    public NetLibEncodingHook() : base(HookName)
    {
        if (WinApi.RunningUnderWine && WinApi.GetACP() == 1252)
        {
            // Fix for old version of Wine (like Wine-GE 8.21), seems to work fine on newer Wine versions tho from my
            // limited testing
            Logging.HookStep(HookName, "Skipping applying due to running under Wine and having correct codepage");
            return;
        }
        
        TargetConstructors = [GetTargetMethod()];
        Postfixes = [AccessTools.Method(typeof(NetLibEncodingHook), nameof(StringStreamCtorPostfix))];
    }

    private static ConstructorInfo? GetTargetMethod()
    {
        ConstructorInfo? targetMethod = AssemblyUtils.CommonOrOsuTypes
            .SelectMany(t => t.GetConstructors(BindingFlags.Public | BindingFlags.Instance) // hardcoded name fallback for clients using Confuser (b20131216)
            ).FirstOrDefault(m => m.GetParameters().Length == 2 &&
                                  m.GetParameters()[0].ParameterType.FullName == "System.String" &&
                                  m.GetParameters()[1].ParameterType.FullName == "System.Object[]" &&
                                  (UsesStreamWriter(m) ||
                                   m.DeclaringType?.Name ==
                                   "StringStream"));
        if (targetMethod == null)
        {
            Logging.HookError(HookName, "Target method not found", !EntryPoint.Config.FirstRun);
            if (EntryPoint.Config.FirstRun)
                EntryPoint.Config.HookNetLibEncoding = false;
            return null;
        }
        
        return targetMethod;
    }
    
    #region Hook

    /// <summary>
    /// Hook for StringStream constructor, that forces writing with UTF-8 without BOM encoding
    /// This has to be a postfix, due to __instance not being initialized yet when it's a prefix
    /// </summary>
    /// <param name="__instance">StringStream instance, here we are taking it as MemoryStream due to StringStream being a custom type that implements MemoryStream</param>
    /// <param name="__0">text</param>
    /// <param name="__1">args</param>
    private static void StringStreamCtorPostfix(ref MemoryStream __instance, string __0, params object[] __1)
    {
        Logging.HookTrigger(HookName);
        try
        {
            __instance.SetLength(0); // Wipe the underlying MemoryStream without disposing it
            
            // Replicate the behavior of original constructor the right way
            StreamWriter sw = new StreamWriter(__instance, new UTF8Encoding(false));
            if (__1.Length == 0)
            {
                sw.Write(__0);
            }
            else
            {
                sw.Write(__0, __1);
            }
        
            sw.Flush();
            __instance.Seek(0, SeekOrigin.Begin);
        }
        catch (Exception e)
        {
            Logging.HookOutput(HookName, $"Error: {e}");
        }
    } 
    
    #endregion
    
    #region Find method
    

    /// <summary>
    /// Check whether the constructor is using StreamWriter
    /// </summary>
    /// <param name="targetMethod"></param>
    /// <returns></returns>
    private static bool UsesStreamWriter(ConstructorInfo targetMethod)
    {
        try
        {
            ILReader reader = new ILReader(targetMethod);

            foreach (ILInstruction instr in reader)
            {
                if (instr.OpCode == OpCodes.Newobj &&
                    instr is InlineMethodInstruction method && // Newobj opcode will contain a method call to a ctor
                    method.Method.DeclaringType is { Name: "StreamWriter" })
                {
                    return true;
                }
            }
        }
        catch
        {
            // Ignore any errors, hopefully nothing goes wrong
            //Debugger.Break();
        }
        
        return false;
    }
    
    #endregion
}
