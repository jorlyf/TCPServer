namespace TCPServer;

public class Packet
{
  public static int MaxRawBufferSize => 1024;
  public int Length { get; private set; }
  public byte[] RawBuffer { get; private set; } = new byte[MaxRawBufferSize];

  private byte[] _data = null!;
  public virtual byte[] Data
  {
    get
    {
      if (_data == null)
      {
        _data = new byte[Length];
        Buffer.BlockCopy(RawBuffer, sizeof(int), _data, 0, Length);
      }
      return _data;
    }
  }

  public Packet(byte[] buff, bool write = true)
  {
    int size = buff.Length + (write ? sizeof(int) : 0);
    if (size > MaxRawBufferSize)
    {
      throw new Exception($"The max packet buffer size is {MaxRawBufferSize}");
    }

    if (write)
    {
      Length = buff.Length;
      if (!BitConverter.TryWriteBytes(RawBuffer, Length))
      {
        throw new Exception("The packet length writing was interrupted");
      }
      buff.CopyTo(RawBuffer, sizeof(int));
    }
    else
    {
      buff.CopyTo(RawBuffer, 0);
      Length = BitConverter.ToInt32(RawBuffer);
    }
  }
}