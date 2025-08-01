using System.Net.Sockets;

namespace TitanicHookManaged.WebSocketCompat;

/// <summary>
/// Fake Socket class to satisfy osu!'s closing needs
/// </summary>
public class FakeSocket : Socket
{
    public FakeSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
    {
    }

    public FakeSocket(SocketInformation socketInformation) : base(socketInformation)
    {
    }
    
    public FakeSocket() : base(new SocketInformation()) {}
    
    public new void Close(int timeout) {}
}
