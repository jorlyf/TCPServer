namespace TCPServer;

public interface IServer
{
  bool IsRunning { get; }

  Task Start(int port);

  Task Stop();


}