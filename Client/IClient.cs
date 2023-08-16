using System.Net.Sockets;

namespace TCPServer;

public interface IClient
{
  Guid Guid { get; }
  TcpClient TcpClient { get; }
  event Action? OnRemove;
  Task CloseAsync();
  Task SendAsync(IPacket packet, CancellationToken token);
  Task<IPacket> ReadAsync(CancellationToken token);
}