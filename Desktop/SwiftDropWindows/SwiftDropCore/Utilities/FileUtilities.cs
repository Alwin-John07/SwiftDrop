using System.IO;

namespace SwiftDropCore.Utilities;

public static class FileUtilities
{
    public static string GetReceivedFolder()
    {
        string folder = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "SwiftDrop",
            "ReceivedFiles");

        Directory.CreateDirectory(folder);

        return folder;
    }

    public static string GetUniqueFilePath(string fileName)
    {
        string folder = GetReceivedFolder();

        string filePath = Path.Combine(folder, fileName);

        if (!File.Exists(filePath))
            return filePath;

        string name = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);

        int counter = 1;

        while (true)
        {
            string newName = $"{name} ({counter}){extension}";

            filePath = Path.Combine(folder, newName);

            if (!File.Exists(filePath))
                return filePath;

            counter++;
        }
    }

    public static string GetTemporaryFilePath(string finalPath)
    {
        return finalPath + ".part";
    }

    public static void FinalizeTransfer(
        string temporaryPath,
        string finalPath)
    {
        if (File.Exists(finalPath))
            File.Delete(finalPath);

        File.Move(temporaryPath, finalPath);
    }

    public static void DeleteTemporaryFile(
        string temporaryPath)
    {
        if (File.Exists(temporaryPath))
            File.Delete(temporaryPath);
    }
}
