namespace SwiftDropCore.Models;

public class DeviceInfo
{
    public string Name { get; set; } = "";

    public string IPAddress { get; set; } = "";

    public int Port { get; set; }

    public DateTime LastSeen { get; set; }

    public override string ToString()
    {
        return $"{Name} ({IPAddress})";
    }
}