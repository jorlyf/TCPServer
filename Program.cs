namespace TCPServer;

public static class Program
{
  private const int _port = 5020;
  private static bool _isExited = false;

  public static async Task Main()
  {
    IServer server = new Server();
    server.Start(_port);

    ConsoleInput();

    await server.StopAsync();
    
    Console.ReadLine();
  }

  private static void ConsoleInput()
  {
    while (!_isExited)
    {
      string? input = Console.ReadLine();
      if (string.IsNullOrEmpty(input)) continue;
      switch (input.ToLower())
      {
        case "stop":
          {
            _isExited = true;
            break;
          }
      }
    }
  }
}
