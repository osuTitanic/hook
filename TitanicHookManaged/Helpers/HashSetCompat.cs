using System;
using System.Collections.Generic;

namespace TitanicHookManaged.Helpers;

/// <summary>
/// Compatibility layer to get HashSet-like functionality on .NET Framework 2.0
/// </summary>
/// <typeparam name="T">Type</typeparam>
public class HashSetCompat<T>
{
    private Dictionary<T, object> _dict = new();

    public bool Add(T value)
    {
        try
        {
            _dict.Add(value, null);
        }
        catch (ArgumentException e)
        {
            return false;
        }
        
        return true;
    }
    
    public bool Contains(T value) => _dict.ContainsKey(value);
    
    public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();
}
