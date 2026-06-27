using SwiftDropCore.Models;

namespace SwiftDropCore.Events;

public static class TransferEvents
{
    // Receiver status
    public static event Action<string>? StatusChanged;

    // Transfer progress (0-100)
    public static event Action<int>? ProgressChanged;

    // Transfer started
    public static event Action<IncomingFile>? TransferStarted;

    // Transfer completed
    public static event Action<IncomingFile>? TransferCompleted;

    // Helper methods

    public static void RaiseStatus(string status)
    {
        StatusChanged?.Invoke(status);
    }

    public static void RaiseProgress(int progress)
    {
        ProgressChanged?.Invoke(progress);
    }

    public static void RaiseTransferStarted(IncomingFile file)
    {
        TransferStarted?.Invoke(file);
    }

    public static void RaiseTransferCompleted(IncomingFile file)
    {
        TransferCompleted?.Invoke(file);
    }
}