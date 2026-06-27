using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using SwiftDropCore.Models;

namespace SwiftDropCore.Networking;

public class DeviceDiscoveryService
{
    private readonly UdpClient _udp =
    new(new IPEndPoint(IPAddress.Any, 5001));

    private readonly ConcurrentDictionary<string, DeviceInfo> _devices =
        new();

    private readonly string _computerName =
        Environment.MachineName;

    public event Action<DeviceInfo>? DeviceDiscovered;

    public IEnumerable<DeviceInfo> Devices =>
    _devices.Values;

    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            UdpReceiveResult result;

            try
            {
                result = await _udp.ReceiveAsync(
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            string message =
                Encoding.UTF8.GetString(result.Buffer);

            if (!message.StartsWith("SWIFTDROP|"))
                continue;

            string[] parts =
                message.Split('|');

            if (parts.Length != 3)
                continue;

            string name = parts[1];

            if (!int.TryParse(parts[2], out int port))
                continue;

            string ip =
                result.RemoteEndPoint.Address.ToString();

            if (IsOwnAnnouncement(name, ip))
                continue;

            bool isNew = false;

DeviceInfo device =
    _devices.AddOrUpdate(
        ip,
        _ =>
        {
            isNew = true;

            return new DeviceInfo
            {
                Name = name,
                IPAddress = ip,
                Port = port,
                LastSeen = DateTime.Now
            };
        },
        (_, existing) =>
        {
            existing.Name = name;
            existing.Port = port;
            existing.LastSeen = DateTime.Now;
            return existing;
        });

if (isNew)
{
    DeviceDiscovered?.Invoke(device);
}
        }
    }

    public void Stop()
    {
        _udp.Close();
    }

    private bool IsOwnAnnouncement(
        string name,
        string ipAddress)
    {
        return string.Equals(
                name,
                _computerName,
                StringComparison.OrdinalIgnoreCase) ||
            GetLocalIpAddresses().Contains(ipAddress);
    }

    private static HashSet<string> GetLocalIpAddresses()
    {
        HashSet<string> addresses = new();

        foreach (NetworkInterface networkInterface in
            NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up)
                continue;

            foreach (UnicastIPAddressInformation address in
                networkInterface.GetIPProperties().UnicastAddresses)
            {
                if (address.Address.AddressFamily ==
                    AddressFamily.InterNetwork)
                {
                    addresses.Add(address.Address.ToString());
                }
            }
        }

        return addresses;
    }
}
