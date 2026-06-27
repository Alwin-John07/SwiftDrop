using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace SwiftDropCore.Networking;

public sealed class ReceiverHost
{
    private readonly TcpListener _listener;
    private readonly FileReceiver _receiver;
    private readonly CancellationTokenSource _broadcasterToken;

    public ReceiverHost(int port = 5000)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _receiver = new FileReceiver();
        _broadcasterToken = new CancellationTokenSource();
    }

    public async Task StartAsync()
    {
        _listener.Start();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=================================");
        Console.WriteLine("SwiftDrop Receiver Started");
        Console.WriteLine("Listening on port 5000...");
        Console.WriteLine("=================================");
        Console.ResetColor();

        _ = DiscoveryBroadcaster.StartAsync(
            _broadcasterToken.Token);

        while (true)
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Waiting for TCP connection...");
                Console.ResetColor();

                TcpClient client =
                    await _listener.AcceptTcpClientAsync();
                    Debug.WriteLine(
    $"TCP ACCEPTED: {client.Client.RemoteEndPoint}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine();
                Console.WriteLine($"TCP Accepted: {client.Client.RemoteEndPoint}");
                Console.WriteLine();
                Console.ResetColor();

                try
                {
                    _receiver.Receive(client);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Receive() completed successfully.");
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Receiver Exception:");
                    Console.WriteLine(ex);
                    Console.ResetColor();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Waiting for next device...");
                Console.WriteLine();
                Console.ResetColor();
            }
            catch (SocketException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Socket Exception: {ex.Message}");
                Console.ResetColor();
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    public void Stop()
    {
        _broadcasterToken.Cancel();
        _listener.Stop();
    }
}