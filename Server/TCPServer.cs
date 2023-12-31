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

  public IClientManager ClientManager { get; } = new ClientManager();

  public BlockingCollection<IPacket> PacketsIn { get; } = new();
  public BlockingCollection<IServerPacket> PacketsOut { get; } = new();

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

  public async Task StopAsync()
  {
    _listenNewConnectionsCTS.Cancel();
    _sendPacketsOutCTS.Cancel();

    await _listenNewConnectionsTask;
    await _sendPacketsOutTask;

    _listenNewConnectionsCTS.Dispose();
    _sendPacketsOutCTS.Dispose();

    _socket.Stop();
    _isRunning = false;

    ClientManager.RemoveAllClients();

    Console.WriteLine("TCP server was stopped");
  }

  public Task SendPacketToClientAsync(Guid clientGuid, IPacket packet)
  {
    IClient? client = ClientManager.GetClient(clientGuid);
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

          ClientManager.AddClient(client);

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
          IPacket packet = await client.ReadAsync(token);
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
      ClientManager.RemoveClient(client.Guid);
    }, token);

  private Task SendPacketsOutAsync(CancellationToken token)
    => Task.Run(() =>
    {
      bool cancelled = false;

      while (!cancelled)
      {
        try
        {
          IServerPacket packet = PacketsOut.Take(token);

          foreach (Guid recipientGuid in packet.RecipientClientGuids)
          {
            SendPacketToClientAsync(recipientGuid, packet);
          }
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
