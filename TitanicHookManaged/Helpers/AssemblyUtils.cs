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
    public static Assembly CommonOrCommonOrOsuAssembly
    {
        get
        {
            if (_commonOrOsuAssembly == null)
            {
                // Check if osu!common is present
                _commonOrOsuAssembly = GetAssembly("osu!common");
                if (_commonOrOsuAssembly == null)
                {
                    // If not, get the osu! assembly
                    _commonOrOsuAssembly = GetAssembly("osu!");
                }
            }

            if (_commonOrOsuAssembly == null)
            {
                Console.WriteLine("Couldn't find neither osu!common or osu!");
                throw new Exception("Couldn't find neither osu!common or osu!");
            }
            return _commonOrOsuAssembly;
        }
    }

    public static Type[] CommonOrOsuTypes
    {
        get
        {
            if (_commonOrOsuTypes == null)
            {
                // Workaround is needed in case reflection can't load an osu!.exe dependency
                try
                {
                    // Try to load normally
                    _commonOrOsuTypes = new List<Type>();
                    _commonOrOsuTypes.AddRange(CommonOrCommonOrOsuAssembly.GetTypes());
                }
                catch (ReflectionTypeLoadException e)
                {
                    // It failed so we start over again but this time we only include the valid types
                    _commonOrOsuTypes = new List<Type>(); // wipe
                    _commonOrOsuTypes.AddRange(e.Types.Where(t => t != null).ToList());
                }
            }
            
            return _commonOrOsuTypes.ToArray();
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

    private static Assembly? _commonOrOsuAssembly;
    private static List<Type>? _commonOrOsuTypes = null;

    #endregion
}
