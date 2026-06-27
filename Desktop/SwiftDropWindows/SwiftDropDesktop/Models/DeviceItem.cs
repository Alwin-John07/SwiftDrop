using SwiftDropCore.Models;

namespace SwiftDropDesktop.Models;

public class DeviceItem
{
    public DeviceInfo Device { get; set; } = new();

    public string DisplayName =>
        $"📱 {Device.Name}";
}