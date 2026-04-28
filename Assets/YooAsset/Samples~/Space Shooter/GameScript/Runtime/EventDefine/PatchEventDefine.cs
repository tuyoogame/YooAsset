using UniFramework.Event;
using YooAsset;

/// <summary>
/// Patch package initialization failed event.
/// </summary>
public sealed class PatchInitializeFailedEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new PatchInitializeFailedEvent());
    }
}

/// <summary>
/// Patch workflow step changed event.
/// </summary>
public sealed class PatchStepChangedEvent : IEventMessage
{
    public string Tips { get; }

    private PatchStepChangedEvent(string tips)
    {
        Tips = tips;
    }

    public static void SendEventMessage(string tips)
    {
        UniEvent.SendMessage(new PatchStepChangedEvent(tips));
    }
}

/// <summary>
/// Update files found event.
/// </summary>
public sealed class PatchFoundUpdateFilesEvent : IEventMessage
{
    public int TotalCount { get; }
    public long TotalSizeBytes { get; }

    private PatchFoundUpdateFilesEvent(int totalCount, long totalSizeBytes)
    {
        TotalCount = totalCount;
        TotalSizeBytes = totalSizeBytes;
    }

    public static void SendEventMessage(int totalCount, long totalSizeBytes)
    {
        UniEvent.SendMessage(new PatchFoundUpdateFilesEvent(totalCount, totalSizeBytes));
    }
}

/// <summary>
/// Download progress updated event.
/// </summary>
public sealed class PatchDownloadUpdatedEvent : IEventMessage
{
    public int TotalDownloadCount { get; }
    public int CurrentDownloadCount { get; }
    public long TotalDownloadSizeBytes { get; }
    public long CurrentDownloadSizeBytes { get; }

    private PatchDownloadUpdatedEvent(DownloadProgressChangedEventArgs updateData)
    {
        TotalDownloadCount = updateData.TotalDownloadCount;
        CurrentDownloadCount = updateData.CurrentDownloadCount;
        TotalDownloadSizeBytes = updateData.TotalDownloadBytes;
        CurrentDownloadSizeBytes = updateData.CurrentDownloadBytes;
    }

    public static void SendEventMessage(DownloadProgressChangedEventArgs updateData)
    {
        UniEvent.SendMessage(new PatchDownloadUpdatedEvent(updateData));
    }
}

/// <summary>
/// Package version request failed event.
/// </summary>
public sealed class PatchPackageVersionRequestFailedEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new PatchPackageVersionRequestFailedEvent());
    }
}

/// <summary>
/// Package manifest update failed event.
/// </summary>
public sealed class PatchPackageManifestUpdateFailedEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new PatchPackageManifestUpdateFailedEvent());
    }
}

/// <summary>
/// Web file download failed event.
/// </summary>
public sealed class PatchWebFileDownloadFailedEvent : IEventMessage
{
    public string FileName { get; }
    public string Error { get; }

    private PatchWebFileDownloadFailedEvent(DownloadErrorEventArgs errorData)
    {
        FileName = errorData.FileName;
        Error = errorData.ErrorInfo;
    }

    public static void SendEventMessage(DownloadErrorEventArgs errorData)
    {
        UniEvent.SendMessage(new PatchWebFileDownloadFailedEvent(errorData));
    }
}