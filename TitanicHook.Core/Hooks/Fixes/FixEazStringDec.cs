using Harmony;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using TitanicHook.Core.Framework;
using TitanicHook.Core.Helpers;

namespace TitanicHook.Core.Hooks.Fixes;

/// <summary>
/// Fix for Eazfuscator's anti-deobfuscation that checks whether the underlying
/// string decryption method is called by an internal Eazfuscator wrapper.
/// Here we replace the check with the core decryptor method rather than the wrapper because that works too.
/// 
/// Credits to Holly @ https://github.com/holly-hacker/EazFixer/blob/master/EazFixer/StacktracePatcher.cs
/// </summary>
public class FixEazStringDec : TitanicPatch
{
    private static readonly string HookName = "sh.Titanic.Hook.FixEazStringDec";

    public FixEazStringDec() : base(HookName)
    {
        TargetMethods = [AccessTools.Method(typeof(StackFrame), nameof(StackFrame.GetMethod))];
        Postfixes = [AccessTools.Method(typeof(FixEazStringDec), nameof(Postfix))];
    }

    private static void Postfix(ref MethodBase? __result)
    {
        if (__result?.Name != "Invoke_StringDec") return; // The call was not made by our string decryption method

        // Replace the method
        __result = ObfHelper.StringObfReference;
    }
}
