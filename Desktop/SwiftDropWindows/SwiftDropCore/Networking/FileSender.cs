using System.Net.Sockets;
using System.Text;
using SwiftDropCore.Protocol;

namespace SwiftDropCore.Networking;

public sealed class FileSender
{
    public async Task SendAsync(
        string ipAddress,
        int port,
        string filePath,
        IProgress<int>? progress = null)
    {
        using TcpClient client = new();

        await client.ConnectAsync(ipAddress, port);

        using NetworkStream stream = client.GetStream();
        using BinaryWriter writer =
            new(stream, Encoding.UTF8, leaveOpen: true);

        FileInfo file = new(filePath);

        // ======================================
        // SwiftDrop Header
        // ======================================

        writer.Write(
            Encoding.ASCII.GetBytes(
                ProtocolConstants.MagicHeader));

        TransferProtocol.WriteInt32BigEndian(
            writer,
            ProtocolConstants.ProtocolVersion);

        TransferProtocol.WriteInt32BigEndian(
            writer,
            1);

        // ======================================
        // File Information
        // ======================================

        byte[] fileNameBytes =
            Encoding.UTF8.GetBytes(file.Name);

        TransferProtocol.WriteInt32BigEndian(
            writer,
            fileNameBytes.Length);

        writer.Write(fileNameBytes);

        TransferProtocol.WriteInt64BigEndian(
            writer,
            file.Length);

        // ======================================
        // File Data
        // ======================================

        byte[] buffer =
            new byte[ProtocolConstants.BufferSize];

        long sent = 0;

        using FileStream input =
            File.OpenRead(file.FullName);

        while (true)
        {
            int bytesRead =
                await input.ReadAsync(buffer);

            if (bytesRead == 0)
                break;

            await stream.WriteAsync(
                buffer.AsMemory(0, bytesRead));

            sent += bytesRead;

            int percent =
                (int)((sent * 100) / file.Length);

            progress?.Report(percent);
        }

        await stream.FlushAsync();
    }
}