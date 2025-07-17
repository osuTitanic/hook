using System;
using System.Reflection;

namespace TitanicHookManaged.Helpers;

public static class AssemblyUtils
{
    /// <summary>
    /// Searches for an assembly with a specified name in the current AppDomain
    /// </summary>
    /// <param name="name">Target assembly name</param>
    /// <returns>Assembly if found, null if not found</returns>
    public static Assembly? GetAssembly(string name)
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
}
