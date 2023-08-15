namespace TCPServer;

public class Packet : IPacket
{
  public static int MaxRawBufferSize { get; set; } = 1024;

  public int Length { get; }

  public byte[] RawBuffer { get; } = new byte[MaxRawBufferSize];

  private byte[] _data = null!;
  public byte[] Data
  {
    get
    {
      if (_data == null)
      {
        _data = new byte[Length];
        RawBuffer.CopyTo(_data, sizeof(int));
      }
      return _data;
    }
  }

  public Guid SenderClientGuid { get; init; } = Guid.Empty;

  public Packet(byte[] buff, bool write)
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

      Buffer.BlockCopy(
        buff,
        0,
        RawBuffer,
        sizeof(int),
        Length);
    }
    else
    {
      Buffer.BlockCopy(
        buff,
        0,
        RawBuffer,
        0,
        MaxRawBufferSize);

      Length = BitConverter.ToInt32(RawBuffer);
    }
  }
}