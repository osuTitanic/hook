using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TitanicHook.Core.Helpers;

public static partial class ObfHelper
{
    private static DecStringDelegate? _decStringDelegate;
    private delegate string? DecStringDelegate(int id);
    
    #region Method Lookups
    /// <summary>
    /// Reference to SmartAssembly string decrypt method
    /// </summary>
    static MethodInfo? _saStringObfReference = AssemblyUtils.OsuTypes
        .Where(t => t.IsClass &&
                    t.IsSealed &&
                    t.IsNotPublic && 
                    !t.IsNested &&
                    t.GetMethods(BindingFlags.Static | BindingFlags.Public).Length == 1
                    )
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public))
        .FirstOrDefault(m => m.GetParameters().Length == 1 && 
                             m.GetParameters()[0].ParameterType.FullName == "System.Int32" && 
                             m.ReturnType.FullName == "System.String" /*&&*/ 
                             /*SigScanning.GetOpcodes(m).StartsWith(_stringObfPrefix)*/); // TODO: control flow obfuscation in that method adds Br opcodes, this is not handled yet but for now the lookup works good enough

    /// <summary>
    /// Reference to Eazfuscator string decrypt method
    /// </summary>
    private static MethodInfo? _eazStringObfReference = AssemblyUtils.OsuTypes
        .Where(t => t.IsClass &&
                    t.IsAbstract &&
                    t.IsSealed &&
                    t.IsNotPublic &&
                    !t.IsNested)
        .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
        .FirstOrDefault(m => m.GetParameters().Length == 1 &&
                             m.GetParameters()[0].ParameterType.FullName == "System.Int32" &&
                             m.ReturnType.FullName == "System.String" /*&&*/
                             /*SigScanning.MethodHasNoInlining(m)*/); // TODO: fucked attribute check
    #endregion
    
    /// <summary>
    /// Reference to the first found string decryption method
    /// </summary>
    private static MethodInfo? _stringObfReference = _saStringObfReference ?? _eazStringObfReference;
    
    public static bool HasStringDecrypt => _stringObfReference != null;
    public static int StringObfToken => _stringObfReference?.MetadataToken ?? 0;
    
    /// <summary>
    /// Decrypts an obfuscated string
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string? DecString(int id)
    {
        // Try to get from cache
        if (_stringCache.TryGetValue(id, out var value))
            return value;

        if (_stringObfReference == null)
            return null;

        if (_decStringDelegate == null)
        {
            _decStringDelegate = CreateStringCallDelegate();
        }

        // It's not in cache, so call the deobfuscation method and add it to cache
        value = _decStringDelegate(id) ?? "";
        _stringCache.Add(id, value);
        
        return value;
    }

    private static DecStringDelegate CreateStringCallDelegate()
    {
        // Create a delegate to quickly call string decryption without reflection invoking
        var dm = new DynamicMethod(
            "Invoke_StringDec", 
            typeof(string), 
            [typeof(int)], 
            _stringObfReference.DeclaringType.Module, 
            true);
            
        var il = dm.GetILGenerator();
            
        il.Emit(OpCodes.Ldarg_0); // Load the int argument
        il.Emit(OpCodes.Call, _stringObfReference); // Call the string decryption method
        il.Emit(OpCodes.Box, typeof(string)); // Box the method into a string
        il.Emit(OpCodes.Ret); // Return the decrypted string
            
        // Create the delegate
        return (DecStringDelegate) dm.CreateDelegate(typeof(DecStringDelegate));
    }
    
    /// <summary>
    /// Cache for deobfuscated strings
    /// </summary>
    private static Dictionary<int, string> _stringCache = new ();
}
