using UniFramework.Event;

/// <summary>
/// Change to home scene event.
/// </summary>
public sealed class SceneChangeToHomeEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new SceneChangeToHomeEvent());
    }
}

/// <summary>
/// Change to battle scene event.
/// </summary>
public sealed class SceneChangeToBattleEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new SceneChangeToBattleEvent());
    }
}