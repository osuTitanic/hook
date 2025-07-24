using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TitanicHookShared;

namespace TitanicHookManaged.Helpers;

public static class AssemblyUtils
{
    /// <summary>
    /// Gets osu!common (if present) or osu! (if osu!common doesn't exist)
    /// </summary>
    public static Assembly CommonOrOsuAssembly
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
                Logging.LogAndShowError("Couldn't find neither osu!common or osu!");
                throw new Exception("Couldn't find neither osu!common or osu!");
            }
            return _commonOrOsuAssembly;
        }
    }
    
    /// <summary>
    /// Gets osu!.exe assembly
    /// </summary>
    public static Assembly OsuAssembly
    {
        get
        {
            if (_osuAssembly == null)
            {
                // Check if osu!common is present
                _osuAssembly = GetAssembly("osu!");
            }

            if (_osuAssembly != null) return _osuAssembly;
            
            Logging.LogAndShowError("Couldn't find osu! assembly");
            throw new Exception("Couldn't find osu! assembly");
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
                    _commonOrOsuTypes.AddRange(CommonOrOsuAssembly.GetTypes());
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

    /// <summary>
    /// Tries to detect the release year of osu! version
    /// </summary>
    /// <param name="osuPath">Path to osu!.exe</param>
    /// <returns>Release year of the osu! assembly if found, or 0 if not</returns>
    public static int DetectOsuYear(string osuPath)
    {
        // Get year from copyright attribute
        // TODO: Maybe use certificate (if supported by OS and signed)

        FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(osuPath);
        var match = Regex.Match(versionInfo.LegalCopyright, @"(\d{4})\b(?!.*\d)");
        if (!match.Success)
            return 0;
        
        return int.Parse(match.Groups[1].Value);
    }
    
    #region Private cache for getters

    private static Assembly? _osuAssembly;
    private static Assembly? _commonOrOsuAssembly;
    private static List<Type>? _commonOrOsuTypes = null;

    #endregion
}
