using System.Collections.Concurrent;

namespace TCPServer;

internal class ClientManager : IClientManager
{
  private readonly ConcurrentDictionary<Guid, IClient> _clients = new();

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
      if (OnAddClient != null)
      {
        Task.Run(() => OnAddClient?.Invoke(client));
      }
    }
  }

  public void RemoveClient(Guid clientGuid)
  {
    if (_clients.TryRemove(clientGuid, out IClient? client))
    {
      if (OnRemoveClient != null)
      {
        Task.Run(() => OnRemoveClient?.Invoke(client));
      }
    }
  }

  public void RemoveAllClients()
  {
    foreach (IClient client in _clients.Values)
    {
      if (OnRemoveClient != null)
      {
        Task.Run(() => OnRemoveClient?.Invoke(client));
      }
    }
    _clients.Clear();
  }
}