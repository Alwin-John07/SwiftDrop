namespace SwiftDropCore.Models;

public class IncomingFile
{
    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string OutputPath { get; set; } = string.Empty;
}