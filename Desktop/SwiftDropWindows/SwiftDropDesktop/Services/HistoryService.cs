using System.Windows;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using SwiftDropDesktop.Models;

namespace SwiftDropDesktop.Services;

public static class HistoryService
{
    private static readonly string AppFolder =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "SwiftDrop");

    private static readonly string HistoryFile =
        Path.Combine(AppFolder, "history.json");

    static HistoryService()
    {
        Directory.CreateDirectory(AppFolder);
    }

    public static ObservableCollection<TransferHistoryItem> Load()
    {
        try
        {
            if (!File.Exists(HistoryFile))
                return new ObservableCollection<TransferHistoryItem>();

            string json = File.ReadAllText(HistoryFile);

            var items = JsonSerializer.Deserialize<List<TransferHistoryItem>>(json);

            return new ObservableCollection<TransferHistoryItem>(
                items ?? new List<TransferHistoryItem>());
        }
        catch
        {
            return new ObservableCollection<TransferHistoryItem>();
        }
    }

    public static void Save(ObservableCollection<TransferHistoryItem> history)
    {
        try
        {
            string json = JsonSerializer.Serialize(
                history,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(HistoryFile, json);
        }
        catch
        {
            // Ignore save errors
        }
    }

    public static void Clear()
    {
        try
        {
            if (File.Exists(HistoryFile))
                File.Delete(HistoryFile);
        }
        catch
        {
            // Ignore delete errors
        }
    }

    public static string GetHistoryFilePath()
    {
        return HistoryFile;
    }
}