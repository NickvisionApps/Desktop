namespace Nickvision.Desktop.Network;

public class DownloadProgress
{
    public DownloadProgress(long totalBytesToReceive, long bytesReceived, bool completed)
    {
        TotalBytesToReceive = totalBytesToReceive;
        BytesReceived = bytesReceived;
        Completed = completed;
    }

    public long TotalBytesToReceive { get; init; }
    public long BytesReceived { get; init; }
    public bool Completed { get; init; }

    public double Percentage => TotalBytesToReceive <= 0 ? 0.0 : (double)BytesReceived / TotalBytesToReceive;
}
