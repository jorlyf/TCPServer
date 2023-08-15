namespace TCPServer;

public interface IPacket
{
  static int MaxRawBufferSize { get; }
  int Length { get; }
  byte[] RawBuffer { get; }
  byte[] Data { get; }
  Guid SenderClientGuid { get; }
}