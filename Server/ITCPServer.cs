using System.Collections.Concurrent;

namespace TCPServer;

public interface IServer
{
  bool IsRunning { get; }

  List<Guid> ConnectedClientGuids { get; }

  BlockingCollection<Packet> PacketsIn { get; }
  BlockingCollection<ServerPacket> PacketsOut { get; }

  void Start(int port);
  Task Stop();
}