using bb_ne_budet;
using WebSocketSharp.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var server = new WebSocketServer("ws://localhost:8080");

        var webSocketService = new WebSocketService();

        server.AddWebSocketService("/matches", () => webSocketService);
        
        server.Start();
        Console.WriteLine("WebSocket server started at ws://localhost:8080/matches");
        
        await webSocketService.StartParsingAndUpdatingAsync();
        
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Shutting down server...");
            server.Stop();
            Environment.Exit(0);
        };
    }
}
