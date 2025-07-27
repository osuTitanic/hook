using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace TitanicHookManaged.Helpers;

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
    
    public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();
}
