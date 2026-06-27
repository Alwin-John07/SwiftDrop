using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SwiftDropCore.Events;
using SwiftDropCore.Models;
using SwiftDropCore.Networking;
using SwiftDropCore.Utilities;
using SwiftDropDesktop.Models;
using SwiftDropDesktop.Services;

namespace SwiftDropDesktop;

public partial class MainWindow : Window
{
    private readonly ReceiverHost _receiverHost;

    private readonly DeviceDiscoveryService _discoveryService =
        new();

    private readonly FileSender _fileSender =
        new();

    private readonly ObservableCollection<DeviceItem> _devices =
        new();

    private readonly ObservableCollection<TransferHistoryItem> _history =
        HistoryService.Load();

    private string? _selectedFile;

    public MainWindow()
    {
        InitializeComponent();

        TransferHistoryList.ItemsSource = _history;
        DeviceList.ItemsSource = _devices;

        DeviceName.Text = "Android Device";
        StatusTitle.Text = "🟢 Ready";
        StatusMessage.Text = "Waiting for Android device...";

        _receiverHost = new ReceiverHost();

        _ = Task.Run(async () =>
        {
            try
            {
                await _receiverHost.StartAsync();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        ex.Message,
                        "Receiver Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
            }
        });

        _ = Task.Run(async () =>
        {
            try
            {
                await _discoveryService.StartAsync(
                    CancellationToken.None);
            }
            catch
            {
            }
        });

        TransferEvents.StatusChanged += OnStatusChanged;
        TransferEvents.TransferStarted += OnTransferStarted;
        TransferEvents.ProgressChanged += OnProgressChanged;
        TransferEvents.TransferCompleted += OnTransferCompleted;

        _discoveryService.DeviceDiscovered +=
            OnDeviceDiscovered;

        BrowseButton.Click += BrowseButton_Click;

        SendButton.Click += SendButton_Click;

        OpenFolderButton.Click +=
            OpenFolderButton_Click;

        Closed += (_, _) =>
        {
            _receiverHost.Stop();
            _discoveryService.Stop();
        };
    }

    private void OnDeviceDiscovered(DeviceInfo device)
    {
        Dispatcher.Invoke(() =>
        {
            if (_devices.Any(d =>
                d.Device.Name == device.Name &&
                d.Device.IPAddress == device.IPAddress &&
                d.Device.Port == device.Port))
            {
                return;
            }

            _devices.Add(new DeviceItem
            {
                Device = device
            });
        });
    }
        private void OnStatusChanged(string status)
    {
        Dispatcher.Invoke(() =>
        {
            StatusMessage.Text = status;

            if (status.Contains("Waiting", StringComparison.OrdinalIgnoreCase))
                StatusTitle.Text = "🟢 Ready";
            else if (status.Contains("connected", StringComparison.OrdinalIgnoreCase))
                StatusTitle.Text = "📱 Connected";
            else if (status.Contains("receiving", StringComparison.OrdinalIgnoreCase))
                StatusTitle.Text = "📥 Receiving";
            else if (status.Contains("sending", StringComparison.OrdinalIgnoreCase))
                StatusTitle.Text = "📤 Sending";
        });
    }

    private void OnTransferStarted(IncomingFile file)
    {
        Dispatcher.Invoke(() =>
        {
            CurrentFile.Text = file.FileName;
            TransferInfo.Text = FormatFileSize(file.FileSize);
            TransferProgress.Value = 0;

            StatusTitle.Text = "📥 Receiving";
            StatusMessage.Text = "Receiving file...";
        });
    }

    private void OnProgressChanged(int progress)
    {
        Dispatcher.Invoke(() =>
        {
            TransferProgress.Value = progress;
            TransferInfo.Text = $"{progress}% Complete";
        });
    }

  private void OnTransferCompleted(IncomingFile file)
{
    Dispatcher.Invoke(() =>
    {
        TransferProgress.Value = 100;

        TransferInfo.Text = "Transfer Complete";

        StatusTitle.Text = "✅ Completed";

        StatusMessage.Text = "Waiting for Android device...";

        _history.Insert(0,
            new TransferHistoryItem
            {
                Icon = GetFileIcon(file.FileName),
                FileName = file.FileName,
                FileSize = FormatFileSize(file.FileSize),
                ReceivedAt = DateTime.Now.ToString("hh:mm tt"),
                FilePath = file.OutputPath
            });

        HistoryService.Save(_history);
    });
}
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new();

        if (dialog.ShowDialog() != true)
            return;

        _selectedFile = dialog.FileName;

        SelectedFileText.Text =
            Path.GetFileName(_selectedFile);
    }

    private async void SendButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_selectedFile == null)
        {
            MessageBox.Show(
                "Please select a file first.",
                "SwiftDrop",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        if (DeviceList.SelectedItem is not DeviceItem device)
        {
            MessageBox.Show(
                "Please select a device.",
                "SwiftDrop",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        try
        {
            TransferProgress.Value = 0;

            CurrentFile.Text =
                Path.GetFileName(_selectedFile);

            StatusTitle.Text = "📤 Sending";

            StatusMessage.Text =
                $"Sending to {device.Device.Name}...";

            Progress<int> progress =
                new(p =>
                {
                    TransferProgress.Value = p;
                    TransferInfo.Text =
                        $"{p}% Complete";
                });

            await _fileSender.SendAsync(
                device.Device.IPAddress,
                device.Device.Port,
                _selectedFile,
                progress);

            TransferProgress.Value = 100;

            TransferInfo.Text =
                "Transfer Complete";

            StatusTitle.Text =
                "✅ Completed";

            StatusMessage.Text =
                "Waiting for Android device...";

            MessageBox.Show(
                "File sent successfully.",
                "SwiftDrop",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Transfer Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }    private void TransferHistoryList_MouseDoubleClick(
        object sender,
        MouseButtonEventArgs e)
    {
        if (TransferHistoryList.SelectedItem is not TransferHistoryItem item)
            return;

        if (!File.Exists(item.FilePath))
        {
            MessageBox.Show(
                "The selected file could not be found.",
                "SwiftDrop",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = item.FilePath,
            UseShellExecute = true
        });
    }

    private TransferHistoryItem? GetSelectedHistoryItem()
    {
        return TransferHistoryList.SelectedItem
            as TransferHistoryItem;
    }

    private void OpenMenuItem_Click(
        object sender,
        RoutedEventArgs e)
    {
        var item = GetSelectedHistoryItem();

        if (item == null || !File.Exists(item.FilePath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = item.FilePath,
            UseShellExecute = true
        });
    }

    private void OpenFolderMenuItem_Click(
        object sender,
        RoutedEventArgs e)
    {
        var item = GetSelectedHistoryItem();

        if (item == null || !File.Exists(item.FilePath))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{item.FilePath}\"",
            UseShellExecute = true
        });
    }

    private void CopyPathMenuItem_Click(
        object sender,
        RoutedEventArgs e)
    {
        var item = GetSelectedHistoryItem();

        if (item == null)
            return;

        Clipboard.SetText(item.FilePath);
    }

    private void RemoveHistoryMenuItem_Click(
        object sender,
        RoutedEventArgs e)
    {
        var item = GetSelectedHistoryItem();

        if (item == null)
            return;

        _history.Remove(item);

        HistoryService.Save(_history);
    }

    private void ClearHistoryMenuItem_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_history.Count == 0)
            return;

        if (MessageBox.Show(
            "Clear all transfer history?",
            "SwiftDrop",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question)
            != MessageBoxResult.Yes)
        {
            return;
        }

        _history.Clear();

        HistoryService.Save(_history);
    }

    private void TransferHistoryList_PreviewMouseRightButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        DependencyObject? source =
            e.OriginalSource as DependencyObject;

        while (source != null &&
               source is not ListViewItem)
        {
            source = VisualTreeHelper.GetParent(source);
        }

        if (source is ListViewItem item)
        {
            item.IsSelected = true;
            item.Focus();
        }
    }
        private static string GetFileIcon(string fileName)
    {
        string extension =
            Path.GetExtension(fileName)
                .ToLowerInvariant();

        return extension switch
        {
            ".png" or ".jpg" or ".jpeg"
                or ".gif"
                or ".bmp"
                or ".webp" => "🖼️",

            ".mp4" or ".avi"
                or ".mkv"
                or ".mov"
                or ".wmv" => "🎬",

            ".mp3"
                or ".wav"
                or ".aac"
                or ".flac" => "🎵",

            ".pdf" => "📕",

            ".zip"
                or ".rar"
                or ".7z" => "📦",

            ".doc"
                or ".docx" => "📘",

            ".xls"
                or ".xlsx" => "📗",

            ".ppt"
                or ".pptx" => "📙",

            ".txt" => "📄",

            _ => "📁"
        };
    }

    private static string FormatFileSize(long bytes)
    {
        double size = bytes;

        string[] units =
        {
            "B",
            "KB",
            "MB",
            "GB",
            "TB"
        };

        int unit = 0;

        while (size >= 1024 &&
               unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.##} {units[unit]}";
    }

    private void AddSentFileToHistory(string filePath)
    {
        FileInfo file = new(filePath);

        _history.Insert(0,
            new TransferHistoryItem
            {
                Icon = GetFileIcon(file.Name),
                FileName = file.Name,
                FileSize = FormatFileSize(file.Length),
                ReceivedAt = DateTime.Now.ToString("hh:mm tt"),
                FilePath = file.FullName
            });

        HistoryService.Save(_history);
    }

    private void OpenFolderButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string folder = FileUtilities.GetReceivedFolder();

        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }
}
