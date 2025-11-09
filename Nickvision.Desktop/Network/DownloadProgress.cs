namespace Nickvision.Desktop.Network;

/// <summary>
///     A class containing information about a download's progress.
/// </summary>
public class DownloadProgress
{
    /// <summary>
    ///     The number of bytes received already.
    /// </summary>
    public long BytesReceived { get; init; }

    /// <summary>
    ///     Whether the download is marked as completed.
    /// </summary>
    public bool Completed { get; init; }

    /// <summary>
    ///     The total number of bytes to be received.
    /// </summary>
    public long TotalBytesToReceive { get; init; }

    /// <summary>
    ///     Constructs a DownloadProgress.
    /// </summary>
    /// <param name="totalBytesToReceive">The total number of bytes to be received</param>
    /// <param name="bytesReceived">The number of bytes received already</param>
    /// <param name="completed">Whether the download is marked as completed</param>
    public DownloadProgress(long totalBytesToReceive, long bytesReceived, bool completed)
    {
        TotalBytesToReceive = totalBytesToReceive;
        BytesReceived = bytesReceived;
        Completed = completed;
    }

    /// <summary>
    ///     The percentage of the download completed.
    /// </summary>
    /// <remarks>Ranges from 0.0 to 1.0</remarks>
    public double Percentage => TotalBytesToReceive <= 0 ? 0.0 : (double)BytesReceived / TotalBytesToReceive;
}
