using System.Net.Sockets;

namespace TCPServer;

public interface IClient
{
  TcpClient TcpClient { get; }

  
}