using System.Collections.Concurrent;

namespace TCPServer;

internal class ClientManager
{
  private readonly ConcurrentDictionary<Guid, IClient> _clients = new();
  private readonly bool IsDebug = true;

  internal List<Guid> ClientGuids => _clients.Keys.ToList();

  internal IClient? GetClient(Guid clientGuid)
  {
    if (_clients.TryGetValue(clientGuid, out IClient? client))
    {
      return client;
    }
    return default;
  }

  internal void AddClient(IClient client)
  {
    _clients.TryAdd(client.Guid, client);
    if (IsDebug)
    {
      Console.WriteLine($"Client {{ {nameof(Guid)} = {client.Guid} }} added");
    }
  }

  internal void RemoveClient(Guid clientGuid)
  {
    if (_clients.TryRemove(clientGuid, out IClient? client))
    {
      if (IsDebug)
      {
        Console.WriteLine($"Client {{ {nameof(Guid)} = {client.Guid} }} removed");
      }
    }
  }
}