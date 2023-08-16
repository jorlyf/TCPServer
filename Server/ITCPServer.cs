using System.Collections.Concurrent;

namespace TCPServer;

public interface IServer
{
  bool IsRunning { get; }

  BlockingCollection<IPacket> PacketsIn { get; }
  BlockingCollection<IServerPacket> PacketsOut { get; }

  void Start(int port);
  Task Stop();
}