namespace TCPServer;

public interface IClientManager
{
  List<Guid> ClientGuids { get; }

  event Action<IClient>? OnAddClient;
  event Action<IClient>? OnRemoveClient;

  IClient? GetClient(Guid clientGuid);
  void AddClient(IClient client);
  void RemoveClient(Guid clientGuid);
  void RemoveAllClients();
}