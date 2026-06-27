namespace SwiftDropCore.Protocol;

public static class ProtocolConstants
{
    // =====================================================
    // Protocol
    // =====================================================

    public const string MagicHeader = "SWFT";

    public const int ProtocolVersion = 1;

    // =====================================================
    // Network
    // =====================================================

    public const int TransferPort = 5000;

    public const int DiscoveryPort = 5001;

    // =====================================================
    // Transfer
    // =====================================================

    public const int BufferSize = 8192;

    // =====================================================
    // Discovery
    // =====================================================

    public const string DiscoveryPrefix = "SWIFTDROP";
}