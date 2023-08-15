namespace TCPServer;

public interface IServerPacket : IPacket
{
  IEnumerable<Guid> RecipientClientGuids { get; }
}