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
public class WebSocketStream : Stream
{
    public Queue<byte[]> ReceiveBuffer = new ();
    public Queue<byte[]> SendBuffer = new ();
    private WebSocket _webSocket;

    public override void Flush()
    {
        throw new System.NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new System.NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new System.NotImplementedException();
    }

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
    public override bool CanRead { get; }
    public override bool CanSeek { get; }
    public override bool CanWrite { get; }
    public override long Length { get; }
    public override long Position { get; set; }

    public WebSocketStream(WebSocket ws)
    {
        _webSocket = ws;
    }
}
#endif
