using System.Net.Sockets;

namespace TCPServer;

public class Client : IClient
{
  public Guid Guid { get; private set; }

  public TcpClient TcpClient { get; private set; }

  private readonly NetworkStream _stream;

  public Client(TcpClient tcp)
  {
    Guid = Guid.NewGuid();
    TcpClient = tcp;

    _stream = TcpClient.GetStream();
  }

  public async Task CloseAsync()
  {
    await _stream.DisposeAsync();
    TcpClient.Dispose();
  }

  public Task SendAsync(Packet packet, CancellationToken token)
  {
    return _stream.WriteAsync(packet.Buffer, token).AsTask();
  }

  public async Task<Packet> ReadAsync(CancellationToken token)
  {
    byte[] buffer = new byte[Packet.MaxBufferSize];
    await _stream.ReadExactlyAsync(buffer, token).AsTask();
    Packet packet = new(buffer, false);
    return packet;
  }
}