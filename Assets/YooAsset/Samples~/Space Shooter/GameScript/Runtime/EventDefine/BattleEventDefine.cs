using UnityEngine;
using UniFramework.Event;

/// <summary>
/// Battle score changed event.
/// </summary>
public sealed class BattleScoreChangedEvent : IEventMessage
{
    public int CurrentScores { get; }

    private BattleScoreChangedEvent(int currentScores)
    {
        CurrentScores = currentScores;
    }

    public static void SendEventMessage(int currentScores)
    {
        UniEvent.SendMessage(new BattleScoreChangedEvent(currentScores));
    }
}

/// <summary>
/// Battle game over event.
/// </summary>
public sealed class BattleGameOverEvent : IEventMessage
{
    public static void SendEventMessage()
    {
        UniEvent.SendMessage(new BattleGameOverEvent());
    }
}

/// <summary>
/// Battle enemy dead event.
/// </summary>
public sealed class BattleEnemyDeadEvent : IEventMessage
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }

    private BattleEnemyDeadEvent(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public static void SendEventMessage(Vector3 position, Quaternion rotation)
    {
        UniEvent.SendMessage(new BattleEnemyDeadEvent(position, rotation));
    }
}

/// <summary>
/// Battle player dead event.
/// </summary>
public sealed class BattlePlayerDeadEvent : IEventMessage
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }

    private BattlePlayerDeadEvent(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public static void SendEventMessage(Vector3 position, Quaternion rotation)
    {
        UniEvent.SendMessage(new BattlePlayerDeadEvent(position, rotation));
    }
}

/// <summary>
/// Battle asteroid explosion event.
/// </summary>
public sealed class BattleAsteroidExplosionEvent : IEventMessage
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }

    private BattleAsteroidExplosionEvent(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public static void SendEventMessage(Vector3 position, Quaternion rotation)
    {
        UniEvent.SendMessage(new BattleAsteroidExplosionEvent(position, rotation));
    }
}

/// <summary>
/// Battle enemy fire bullet event.
/// </summary>
public sealed class BattleEnemyFireBulletEvent : IEventMessage
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }

    private BattleEnemyFireBulletEvent(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public static void SendEventMessage(Vector3 position, Quaternion rotation)
    {
        UniEvent.SendMessage(new BattleEnemyFireBulletEvent(position, rotation));
    }
}

/// <summary>
/// Battle player fire bullet event.
/// </summary>
public sealed class BattlePlayerFireBulletEvent : IEventMessage
{
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }

    private BattlePlayerFireBulletEvent(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public static void SendEventMessage(Vector3 position, Quaternion rotation)
    {
        UniEvent.SendMessage(new BattlePlayerFireBulletEvent(position, rotation));
    }
}