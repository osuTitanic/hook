using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TitanicHookManaged.Helpers;

public static class AssemblyUtils
{
    /// <summary>
    /// Gets osu!common (if present) or osu!
    /// </summary>
    public static Assembly OsuOrCommonAssembly
    {
        get
        {
            if (_osuOrCommonAssembly == null)
            {
                // Check if osu!common is present
                _osuOrCommonAssembly = GetAssembly("osu!common");
                if (_osuOrCommonAssembly == null)
                {
                    // If not, get the osu! assembly
                    _osuOrCommonAssembly = GetAssembly("osu!");
                }
            }

            if (_osuOrCommonAssembly == null)
            {
                Console.WriteLine("Couldn't find neither osu!common or osu!");
                throw new Exception("Couldn't find neither osu!common or osu!");
            }
            return _osuOrCommonAssembly;
        }
    }

    public static Type[] OsuOrCommonTypes
    {
        get
        {
            if (_osuOrCommonTypes == null)
            {
                // Workaround is needed in case reflection can't load an osu!.exe dependency
                try
                {
                    // Try to load normally
                    _osuOrCommonTypes = new List<Type>();
                    _osuOrCommonTypes.AddRange(OsuOrCommonAssembly.GetTypes());
                }
                catch (ReflectionTypeLoadException e)
                {
                    // It failed so we start over again but this time we only include the valid types
                    _osuOrCommonTypes = new List<Type>(); // wipe
                    _osuOrCommonTypes.AddRange(e.Types.Where(t => t != null).ToList());
                }
            }
            
            return _osuOrCommonTypes.ToArray();
        }
    }
    
    /// <summary>
    /// Searches for an assembly with a specified name in the current AppDomain
    /// </summary>
    /// <param name="name">Target assembly name</param>
    /// <returns>Assembly if found, null if not found</returns>
    private static Assembly? GetAssembly(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetName().Name == name)
            {
                return assembly;
            }
        }
        return null;
    }
    
    #region Private cache for getters

    private static Assembly? _osuOrCommonAssembly;
    private static List<Type>? _osuOrCommonTypes = null;

    #endregion
}
