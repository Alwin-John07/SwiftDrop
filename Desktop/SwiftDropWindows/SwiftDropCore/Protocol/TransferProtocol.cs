namespace SwiftDropCore.Protocol;

using System.IO;
using System.Text;
using SwiftDropCore.Models;


public static class TransferProtocol
{
    // =====================================================
    // Primitive Readers
    // =====================================================

    public static int ReadInt32BigEndian(BinaryReader reader)
    {
        byte[] buffer = reader.ReadBytes(4);

        if (buffer.Length != 4)
            throw new EndOfStreamException("Unable to read Int32.");

        if (BitConverter.IsLittleEndian)
            Array.Reverse(buffer);

        return BitConverter.ToInt32(buffer, 0);
    }

    public static long ReadInt64BigEndian(BinaryReader reader)
    {
        byte[] buffer = reader.ReadBytes(8);

        if (buffer.Length != 8)
            throw new EndOfStreamException("Unable to read Int64.");

        if (BitConverter.IsLittleEndian)
            Array.Reverse(buffer);

        return BitConverter.ToInt64(buffer, 0);
    }

    // =====================================================
    // Protocol Readers
    // =====================================================

    public static void ReadMagicHeader(BinaryReader reader)
    {
        byte[] magicBytes = reader.ReadBytes(4);

        if (magicBytes.Length != 4)
            throw new Exception("Invalid protocol header.");

        string magic = Encoding.ASCII.GetString(magicBytes);

        if (magic != ProtocolConstants.MagicHeader)
            throw new Exception("Unknown protocol.");
    }

    public static void ReadProtocolVersion(BinaryReader reader)
    {
        int version = ReadInt32BigEndian(reader);

        if (version != ProtocolConstants.ProtocolVersion)
            throw new Exception(
                $"Unsupported protocol version: {version}");
    }

    public static int ReadFileCount(BinaryReader reader)
    {
        return ReadInt32BigEndian(reader);
    }

    public static IncomingFile ReadFileInfo(BinaryReader reader)
    {
        int nameLength = ReadInt32BigEndian(reader);

        byte[] nameBytes = reader.ReadBytes(nameLength);

        string fileName =
            Encoding.UTF8.GetString(nameBytes);

        long fileSize =
            ReadInt64BigEndian(reader);

        return new IncomingFile
        {
            FileName = fileName,
            FileSize = fileSize
        };
    }

    // =====================================================
    // Primitive Writers (future use)
    // =====================================================

    public static void WriteInt32BigEndian(
        BinaryWriter writer,
        int value)
    {
        byte[] buffer = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(buffer);

        writer.Write(buffer);
    }

    public static void WriteInt64BigEndian(
        BinaryWriter writer,
        long value)
    {
        byte[] buffer = BitConverter.GetBytes(value);

        if (BitConverter.IsLittleEndian)
            Array.Reverse(buffer);

        writer.Write(buffer);
    }
}