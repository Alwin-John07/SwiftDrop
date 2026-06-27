namespace SwiftDropCore.Networking;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

public static class DiscoveryBroadcaster
{
    private const int DISCOVERY_PORT = 5001;
    private const int FILE_TRANSFER_PORT = 5000;

    public static async Task StartAsync(CancellationToken token)
    {
        using UdpClient udp = new UdpClient();

        udp.EnableBroadcast = true;

        string computerName = Environment.MachineName;

        while (!token.IsCancellationRequested)
        {
            string message =
                $"SWIFTDROP|{computerName}|{FILE_TRANSFER_PORT}";

            byte[] data = Encoding.UTF8.GetBytes(message);

            foreach (IPEndPoint endPoint in GetBroadcastEndPoints())
            {
                await udp.SendAsync(data, data.Length, endPoint);
            }

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"Broadcasting: {message}");

            await Task.Delay(1000, token);
        }
    }

    private static IEnumerable<IPEndPoint> GetBroadcastEndPoints()
    {
        foreach (NetworkInterface networkInterface in
            NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
                continue;

            IPInterfaceProperties properties =
                networkInterface.GetIPProperties();

            foreach (UnicastIPAddressInformation address in
                properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily != AddressFamily.InterNetwork ||
                    address.IPv4Mask == null)
                {
                    continue;
                }

                yield return new IPEndPoint(
                    GetBroadcastAddress(address.Address, address.IPv4Mask),
                    DISCOVERY_PORT);
            }
        }
    }

    private static IPAddress GetBroadcastAddress(
        IPAddress address,
        IPAddress subnetMask)
    {
        byte[] addressBytes = address.GetAddressBytes();
        byte[] maskBytes = subnetMask.GetAddressBytes();
        byte[] broadcastBytes = new byte[addressBytes.Length];

        for (int i = 0; i < broadcastBytes.Length; i++)
        {
            broadcastBytes[i] =
                (byte)(addressBytes[i] | ~maskBytes[i]);
        }

        return new IPAddress(broadcastBytes);
    }
}
