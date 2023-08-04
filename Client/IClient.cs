using System.Net.Sockets;

namespace TCPServer;

public interface IClient
{
  Guid Guid { get; }
  TcpClient TcpClient { get; }
  Task CloseAsync();
  Task SendAsync(Packet packet, CancellationToken token);
  Task<Packet> ReadAsync(CancellationToken token);
}