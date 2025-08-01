#if NET35
using System.IO;
using System.Net;
using System.Net.Sockets;
using WebSocketSharp;

namespace TitanicHookManaged.WebSocketCompat;

public class TcpOverWebsocket : TcpClient
{
    private WebSocketStream _stream;
    private WebSocket? _webSocket;
    
    public TcpOverWebsocket(IPEndPoint endPoint, int msTimeout)
    {
        _webSocket = new WebSocket($"wss://server.{EntryPoint.Config.ServerName}/ws");
        _stream = new WebSocketStream(_webSocket);
        _webSocket.OnMessage += (sender, e) =>
        {
            _stream.ReceiveBuffer.Enqueue(e.RawData);
        };
    }

    public new NetworkStream GetStream() => _stream;
    public new bool Connected => _webSocket?.IsAlive ?? false;
    public new Socket Client => new FakeSocket();

    public new void Close()
    {
        _webSocket?.Close();
        _webSocket = null;
    }
}
#endif
