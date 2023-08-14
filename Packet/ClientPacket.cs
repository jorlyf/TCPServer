namespace TCPServer;

public class ClientPacket
{
  public static int MaxBufferSize => 1024;
  public int Length { get; private set; }
  public byte[] Buffer { get; private set; } = new byte[MaxBufferSize];

  public ClientPacket(byte[] buff, bool write = true)
  {
    int size = buff.Length + (write ? sizeof(int) : 0);
    if (size > MaxBufferSize)
    {
      throw new Exception($"The max packet buffer size is {MaxBufferSize}");
    }

    if (write)
    {
      Length = buff.Length;
      if (!BitConverter.TryWriteBytes(Buffer, Length))
      {
        throw new Exception("The packet length writing was interrupted");
      }
      buff.CopyTo(Buffer, sizeof(int));
    }
    else
    {
      buff.CopyTo(Buffer, 0);
      Length = BitConverter.ToInt32(Buffer);
    }
  }
}