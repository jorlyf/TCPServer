using System.Collections.Concurrent;

namespace TCPServer;

public class ClientManager
{
  private readonly ConcurrentDictionary<Guid, IClient> _clients = new();
  private readonly bool IsDebug = true;

  public List<Guid> ClientGuids => _clients.Keys.ToList();

  public event Action<IClient>? OnAddClient;
  public event Action<IClient>? OnRemoveClient;

  public IClient? GetClient(Guid clientGuid)
  {
    if (_clients.TryGetValue(clientGuid, out IClient? client))
    {
      return client;
    }
    return default;
  }

  public void AddClient(IClient client)
  {
    if (_clients.TryAdd(client.Guid, client))
    {
      OnAddClient?.Invoke(client);
      if (IsDebug)
      {
        Console.WriteLine($"Client {{ {nameof(Guid)} = {client.Guid} }} added");
      }
    }
  }

  public void RemoveClient(Guid clientGuid)
  {
    if (_clients.TryRemove(clientGuid, out IClient? client))
    {
      OnRemoveClient?.Invoke(client);
      if (IsDebug)
      {
        Console.WriteLine($"Client {{ {nameof(Guid)} = {client.Guid} }} removed");
      }
    }
  }
}