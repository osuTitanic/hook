#if NET20
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using WebSocketSharp;

namespace TitanicHookManaged.WebSocketCompat;

/// <summary>
/// NetworkStream for WebSocket operations
/// </summary>
public class WebSocketStream : NetworkStream
{
    public Queue<byte[]> ReceiveBuffer = new ();
    public Queue<byte[]> SendBuffer = new ();
    private WebSocket _webSocket;

    /// <summary>
    /// Dequeue bytes from the ReceiveBuffer and write these into a buffer
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public override int Read([In] [Out] byte[] buffer, int offset, int size)
    {
        byte[] bytes = ReceiveBuffer.Dequeue();
        int i;
        for (i = 0; i < bytes.Length; i++)
        {
            if (i > size)
                return i;
            buffer[offset + i] = bytes[i];
        }

        return i;
    }

    /// <summary>
    /// Sends data over the WebSocket connection
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    public override void Write(byte[] buffer, int offset, int size)
    {
        List<byte> bytes = [];
        for (int i = 0; i < size; i++) bytes.Add(buffer[offset + i]);
        _webSocket.Send(bytes.ToArray());
    }

    public new bool DataAvailable => ReceiveBuffer.Count > 0;
    public new bool CanRead => true;
    public new bool CanWrite => true;

    public WebSocketStream(WebSocket ws) : base(new Socket(AddressFamily.Unknown, SocketType.Unknown, ProtocolType.Unknown))
    {
        _webSocket = ws;
    }
    
    public WebSocketStream(Socket socket) : base(socket)
    {
    }

    public WebSocketStream(Socket socket, bool ownsSocket) : base(socket, ownsSocket)
    {
    }

    public WebSocketStream(Socket socket, FileAccess access) : base(socket, access)
    {
    }

    public WebSocketStream(Socket socket, FileAccess access, bool ownsSocket) : base(socket, access, ownsSocket)
    {
    }
}
#endif
