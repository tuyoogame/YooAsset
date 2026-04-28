using UniFramework.Event;

/// <summary>
/// User retry package initialization event.
/// </summary>
public sealed class UserTryInitializePackageEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new UserTryInitializePackageEvent());
    }
}

/// <summary>
/// User begin downloading web files event.
/// </summary>
public sealed class UserBeginDownloadWebFilesEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new UserBeginDownloadWebFilesEvent());
    }
}

/// <summary>
/// User retry package version request event.
/// </summary>
public sealed class UserTryRequestPackageVersionEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new UserTryRequestPackageVersionEvent());
    }
}

/// <summary>
/// User retry package manifest update event.
/// </summary>
public sealed class UserTryUpdatePackageManifestEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new UserTryUpdatePackageManifestEvent());
    }
}

/// <summary>
/// User retry web file download event.
/// </summary>
public sealed class UserTryDownloadWebFilesEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new UserTryDownloadWebFilesEvent());
    }
}