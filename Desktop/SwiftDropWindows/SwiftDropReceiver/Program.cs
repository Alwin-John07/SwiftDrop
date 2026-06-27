using SwiftDropCore.Networking;

Console.Title = "SwiftDrop Receiver";

Console.ForegroundColor = ConsoleColor.Green;

Console.WriteLine("===================================");
Console.WriteLine("      SwiftDrop Receiver");
Console.WriteLine("===================================");
Console.WriteLine();

ReceiverHost host = new();

await host.StartAsync();