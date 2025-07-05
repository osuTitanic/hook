using System.Runtime.InteropServices;

namespace TitanicHookManaged;

[StructLayout(LayoutKind.Sequential)]
public struct SockaddrIn
{
    public short sin_family;
    public ushort sin_port;
    public uint sin_addr;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] sin_zero;
}
