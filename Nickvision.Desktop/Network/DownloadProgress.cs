namespace Nickvision.Desktop.Network;

public class DownloadProgress
{
    public long BytesReceived { get; init; }

    public bool Completed { get; init; }

    public long TotalBytesToReceive { get; init; }

    public DownloadProgress(long totalBytesToReceive, long bytesReceived, bool completed)
    {
        TotalBytesToReceive = totalBytesToReceive;
        BytesReceived = bytesReceived;
        Completed = completed;
    }

    public double Percentage => TotalBytesToReceive <= 0 ? 0.0 : (double)BytesReceived / TotalBytesToReceive;
}
