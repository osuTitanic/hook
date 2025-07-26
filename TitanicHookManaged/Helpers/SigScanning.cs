using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ClrTest.Reflection;

namespace TitanicHookManaged.Helpers;

public class SigScanning
{
    /// <summary>
    /// Gets all OpCodes from a method
    /// </summary>
    /// <param name="method">Method</param>
    /// <returns>All OpCodes</returns>
    public static OpCode[] GetOpcodes(MethodInfo method)
    {
        // TODO: Fix this for builds using SmartAssembly control flow obfuscation.
        // SmartAssembly seems to add Br.s opcodes to confuse IL readers
        ILReader reader = new ILReader(method);
        return reader.Select(instr => instr.OpCode)
            .ToArray();
    }
}
