using System.Diagnostics;
using System.Net.Sockets;
using SwiftDropCore.Events;
using SwiftDropCore.Models;
using SwiftDropCore.Protocol;
using SwiftDropCore.Utilities;

namespace SwiftDropCore.Networking;

public sealed class FileReceiver
{
    public void Receive(TcpClient client)
    {
        Debug.WriteLine("========== FileReceiver ENTER ==========");

        NetworkStream stream = client.GetStream();
        BinaryReader reader = new BinaryReader(stream);

        try
        {
            TransferEvents.RaiseStatus("Android connected");

            Debug.WriteLine("Reading magic header...");
            TransferProtocol.ReadMagicHeader(reader);
            Debug.WriteLine("Magic header OK");

            Debug.WriteLine("Reading protocol version...");
            TransferProtocol.ReadProtocolVersion(reader);
            Debug.WriteLine("Protocol version OK");

            int fileCount = TransferProtocol.ReadFileCount(reader);

            Debug.WriteLine($"File count = {fileCount}");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Receiving {fileCount} file(s)");
            Console.WriteLine();

            for (int i = 0; i < fileCount; i++)
            {
                IncomingFile file =
                    TransferProtocol.ReadFileInfo(reader);

                Debug.WriteLine($"Receiving file: {file.FileName}");
                Debug.WriteLine($"File size: {file.FileSize}");

                TransferEvents.RaiseTransferStarted(file);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Filename : {file.FileName}");
                Console.WriteLine($"Filesize : {file.FileSize:N0} bytes");
                Console.WriteLine();

                string finalPath =
                    FileUtilities.GetUniqueFilePath(file.FileName);

                string tempPath =
                    FileUtilities.GetTemporaryFilePath(finalPath);

                file.OutputPath = finalPath;

                byte[] buffer =
                    new byte[ProtocolConstants.BufferSize];

                long remaining = file.FileSize;
                long received = 0;

                int lastProgress = -1;

                using FileStream fileStream =
                    File.Create(tempPath);

                while (remaining > 0)
                {
                    int bytesToRead =
                        (int)Math.Min(
                            buffer.Length,
                            remaining);

                    int bytesRead =
                        stream.Read(
                            buffer,
                            0,
                            bytesToRead);

                    if (bytesRead <= 0)
                        throw new EndOfStreamException(
                            "Connection lost during transfer.");

                    fileStream.Write(
                        buffer,
                        0,
                        bytesRead);

                    remaining -= bytesRead;
                    received += bytesRead;

                    int progress =
                        (int)((received * 100) /
                        file.FileSize);

                    if (progress != lastProgress)
                    {
                        Console.Write(
                            $"\rReceiving... {progress}%");

                        TransferEvents.RaiseProgress(
                            progress);

                        lastProgress = progress;
                    }
                }

                fileStream.Close();

                Debug.WriteLine("File write complete");

                FileUtilities.FinalizeTransfer(
                    tempPath,
                    finalPath);

                Debug.WriteLine(
                    $"Saved to {finalPath}");

                TransferEvents.RaiseTransferCompleted(
                    file);

                Console.WriteLine();

                Console.ForegroundColor =
                    ConsoleColor.Green;

                Console.WriteLine();
                Console.WriteLine(
                    "Transfer Complete");
                Console.WriteLine(finalPath);
                Console.WriteLine();
            }

            TransferEvents.RaiseStatus(
                "Waiting for Android device...");

            Debug.WriteLine(
                "========== FileReceiver EXIT ==========");
        }
        finally
        {
            Debug.WriteLine("Closing client socket");

            client.Close();
        }
    }
}