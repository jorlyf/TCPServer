using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace TCPServer;

public class Server : IServer
{
  private volatile bool _isRunning = false;
  public bool IsRunning => _isRunning;

  private TcpListener _socket = null!;

  private Task _listenNewConnectionsTask = null!;
  private CancellationTokenSource _listenNewConnectionsCTS = null!;

  private Task _sendPacketsOutTask = null!;
  private CancellationTokenSource _sendPacketsOutCTS = null!;

  private readonly ClientManager _clientManager = new();

  public List<Guid> ConnectedClientGuids => _clientManager.ClientGuids;

  public BlockingCollection<Packet> PacketsIn { get; } = new();
  public BlockingCollection<ServerPacket> PacketsOut { get; } = new();

  public void Start(int port)
  {
    if (_isRunning) throw new Exception("TCPServer is already started");
    _isRunning = true;

    _socket = new TcpListener(IPAddress.Any, port);
    _socket.Start();

    _listenNewConnectionsCTS = new();
    _listenNewConnectionsTask = ListenNewConnectionsAsync(_listenNewConnectionsCTS.Token);

    _sendPacketsOutCTS = new();
    _sendPacketsOutTask = SendPacketsOutAsync(_sendPacketsOutCTS.Token);
  }

  public async Task Stop()
  {
    _listenNewConnectionsCTS.Cancel();
    _sendPacketsOutCTS.Cancel();

    await _listenNewConnectionsTask;
    await _sendPacketsOutTask;

    _listenNewConnectionsCTS.Dispose();
    _sendPacketsOutCTS.Dispose();

    _socket.Stop();
    _isRunning = false;

    Console.WriteLine("TCP server was stopped");
  }

  public Task SendPacketToClientAsync(Guid clientGuid, Packet packet)
  {
    IClient? client = _clientManager.GetClient(clientGuid);
    if (client == null)
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

      return client.CloseAsync();
    }
  }

  private Task ListenNewConnectionsAsync(CancellationToken token)
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

          _clientManager.AddClient(client);

          Task handle = HandleClientConnectionAsync(client, token);
          processConnectionTasks.Add(handle);
        }
        catch (OperationCanceledException)
        {
          cancelled = true;
          Task.WaitAll(processConnectionTasks.ToArray());
        }
        catch (Exception ex)
        {
          Console.WriteLine($"ListenNewConnectionsAsync exception {ex.Message}");
        }
      }
    }, token);

  private Task HandleClientConnectionAsync(Client client, CancellationToken token)
    => Task.Run(async () =>
    {
      bool cancelled = false;

      while (!cancelled)
      {
        try
        {
          Packet packet = await client.ReadAsync(token);
          PacketsIn.Add(packet);
        }
        catch (OperationCanceledException)
        {
          cancelled = true;
        }
        catch (Exception)
        {
          cancelled = true;
        }
      }

      await client.CloseAsync();
      _clientManager.RemoveClient(client.Guid);
    }, token);

  private Task SendPacketsOutAsync(CancellationToken token)
    => Task.Run(() =>
    {
      bool cancelled = false;

      while (!cancelled)
      {
        try
        {
          ServerPacket packet = PacketsOut.Take(token);

          List<Task> tasks = new();

          foreach (Guid recipientGuid in packet.RecipientGuids)
          {
            Task task = SendPacketToClientAsync(recipientGuid, packet);
            tasks.Add(task);
          }

          // Task.WaitAll(tasks.ToArray());
        }
        catch (OperationCanceledException)
        {
          cancelled = true;
        }
        catch (Exception ex)
        {
          cancelled = true;
          Console.WriteLine($"SendPacketsOutAsync exception {ex.Message}");
        }
      }
    }, token);
}
