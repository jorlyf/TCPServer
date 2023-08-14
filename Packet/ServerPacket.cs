namespace TCPServer;

public class ServerPacket : Packet
{
  public List<Guid> RecipientGuids;

  public ServerPacket(List<Guid> recipientGuids, byte[] buff) : base(buff, true)
  {
    RecipientGuids = recipientGuids;
  }
}