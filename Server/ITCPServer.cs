using System.Collections.Concurrent;

namespace TCPServer;

public interface IServer
{
  bool IsRunning { get; }

  ConcurrentQueue<ClientPacket> PacketsIn { get; }
  ConcurrentQueue<ServerPacket> PacketsOut { get; }

  Task Start(int port);
  Task Stop();
  Task SendPacketToClient(Guid clientGuid, ClientPacket packet);
}