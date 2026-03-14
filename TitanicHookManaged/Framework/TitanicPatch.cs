using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using TitanicHookManaged.Helpers;

namespace TitanicHookManaged.Framework;

/// <summary>
/// Base class which allows for patching multiple methods with multiple
/// prefixes/postfixes/transpilers.
/// </summary>
public abstract class TitanicPatch
{
    /// <summary>
    /// Name of the patch
    /// </summary>
    public string HookName { get; set; }

    public List<MethodInfo> TargetMethods { get; set; } = [];
    public List<ConstructorInfo> TargetConstructors { get; set; } = [];
    public List<MethodInfo> Prefixes { get; set; } = [];
    public List<MethodInfo> Postfixes { get; set; } = [];
    public List<MethodInfo> Transpilers { get; set; } = [];

    private HarmonyInstance Harmony;
    
    public TitanicPatch(string hookName)
    {
        HookName = hookName;
        Harmony = HarmonyInstance.Create(HookName);
    }

    public void Patch()
    {
        foreach (MethodInfo method in TargetMethods)
        {
            if (method == null)
                continue;
            
            Logging.Info($"[{HookName}] Patching ${method.Name}");

            foreach (MethodInfo prefix in Prefixes)
            {
                if (prefix == null)
                    continue;
                
                Logging.Info($"[{HookName}] Applying prefix {prefix.Name} onto {method.Name}");
                try
                {
                    Harmony.Patch(method, prefix: new HarmonyMethod(prefix));
                }
                catch (Exception e)
                {
                    Logging.HookError(HookName, $"Fail at prefixing {method.Name} with {prefix.Name}: {e}");
                }
            }
            
            foreach (MethodInfo postfix in Postfixes)
            {
                if (postfix == null)
                    continue;
                
                Logging.Info($"[{HookName}] Applying postfix {postfix.Name} onto {method.Name}");
                try
                {
                    Harmony.Patch(method, postfix: new HarmonyMethod(postfix));
                }
                catch (Exception e)
                {
                    Logging.HookError(HookName, $"Fail at postfixing {method.Name} with {postfix.Name}: {e}");
                }
            }
            
            foreach (MethodInfo transpiler in Transpilers)
            {
                if (transpiler == null)
                    continue;
                
                Logging.Info($"[{HookName}] Transpiling {method.Name} with {transpiler.Name}");
                try
                {
                    Harmony.Patch(method, transpiler: new HarmonyMethod(transpiler));
                }
                catch (Exception e)
                {
                    Logging.HookError(HookName, $"Fail at transpiling {method.Name} with {transpiler.Name}: {e}");
                }
            }
        }
        
        foreach (ConstructorInfo constructor in TargetConstructors)
        {
            if (constructor == null)
                continue;
            
            Logging.Info($"[{HookName}] Patching constructor ${constructor.Name}");

            foreach (MethodInfo prefix in Prefixes)
            {
                if (prefix == null)
                    continue;
                
                Logging.Info($"[{HookName}] Applying prefix {prefix.Name} onto {constructor.Name}");
                try
                {
                    Harmony.Patch(constructor, prefix: new HarmonyMethod(prefix));
                }
                catch (Exception e)
                {
                    Logging.HookError(HookName, $"Fail at prefixing {constructor.Name} with {prefix.Name}: {e}");
                }
            }
            
            foreach (MethodInfo postfix in Postfixes)
            {
                if (postfix == null)
                    continue;
                
                Logging.Info($"[{HookName}] Applying postfix {postfix.Name} onto {constructor.Name}");
                try
                {
                    Harmony.Patch(constructor, postfix: new HarmonyMethod(postfix));
                }
                catch (Exception e)
                {
                    Logging.HookError(HookName, $"Fail at postfixing {constructor.Name} with {postfix.Name}: {e}");
                }
            }
            
            foreach (MethodInfo transpiler in Transpilers)
            {
                if (transpiler == null)
                    continue;
                
                Logging.Info($"[{HookName}] Transpiling {constructor.Name} with {transpiler.Name}");
                try
                {
                    Harmony.Patch(constructor, transpiler: new HarmonyMethod(transpiler));
                }
                catch (Exception e)
                {
                    Logging.HookError(HookName, $"Fail at transpiling {constructor.Name} with {transpiler.Name}: {e}");
                }
            }
        }
    }
}
