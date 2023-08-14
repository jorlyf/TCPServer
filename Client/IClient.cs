using System.Net.Sockets;

namespace TCPServer;

public interface IClient
{
  Guid Guid { get; }
  TcpClient TcpClient { get; }
  Task CloseAsync();
  Task SendAsync(ClientPacket packet, CancellationToken token);
  Task<ClientPacket> ReadAsync(CancellationToken token);
}