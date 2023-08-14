using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace TCPServer;

public class Server : IServer
{
  private volatile bool _isRunning = false;
  public bool IsRunning => _isRunning;

  private TcpListener _socket = null!;
  private CancellationTokenSource _listenConnectionsCTS = null!;

  private Task _listenTCPConnectionsTask = null!;

  private readonly ConcurrentDictionary<Guid, Client> _clients = new();

  public ConcurrentQueue<ClientPacket> PacketsIn { get; } = new();
  public ConcurrentQueue<ServerPacket> PacketsOut { get; } = new();

  public Task Start(int port)
  {
    if (_isRunning) throw new Exception("TCPServer is already started");
    _isRunning = true;

    _socket = new TcpListener(IPAddress.Any, port);
    _socket.Start();

    _listenConnectionsCTS = new();

    _listenTCPConnectionsTask = ListenTCPConnections(_listenConnectionsCTS.Token);
    return _listenTCPConnectionsTask;
  }

  public async Task Stop()
  {
    _isRunning = false;
    _listenConnectionsCTS.Cancel();
    Thread.Sleep(1000);
    _listenConnectionsCTS.Dispose();

    await _listenTCPConnectionsTask;

    _socket.Stop();

    Console.WriteLine("TCP server was stopped");
  }

  public Task SendPacketToClient(Guid clientGuid, ClientPacket packet)
  {
    if (!_clients.TryGetValue(clientGuid, out Client? client))
    {
      return Task.CompletedTask;
    }

    try
    {
      return client.SendAsync(packet, CancellationToken.None);
    }
    catch (Exception ex)
    {
      Console.WriteLine("SendPacketToClient exception");
      Console.WriteLine(ex.Message);
      return Task.CompletedTask;
    }
  }

  private Task ListenTCPConnections(CancellationToken token)
    => Task.Run(async () =>
    {
      bool cancelled = false;
      List<Task> processConnectionTasks = new();

      while (!cancelled)
      {
        try
        {
          TcpClient tcpClient = await _socket.AcceptTcpClientAsync(token);

          Client client = new(tcpClient);

          if (_clients.TryAdd(client.Guid, client))
          {
            Task handle = HandleClientConnection(client, token);
            processConnectionTasks.Add(handle);
          }
        }
        catch (OperationCanceledException)
        {
          cancelled = true;
          Task.WaitAll(processConnectionTasks.ToArray());
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex);
        }
      }
    }, token);

  private Task HandleClientConnection(Client client, CancellationToken token)
    => Task.Run(async () =>
    {
      bool cancelled = false;

      while (!cancelled)
      {
        try
        {
          ClientPacket packet = await client.ReadAsync(CancellationToken.None);
          PacketsIn.Enqueue(packet);
          Console.WriteLine("Packet accepted");
        }
        catch (OperationCanceledException)
        {
          cancelled = true;
        }
        catch (Exception ex)
        {
          cancelled = true;
          Console.WriteLine($"HandleClientConnection exception.\n{ex.Message}");
        }
      }

      await client.CloseAsync();
    }, token);
}
