using System;
using System.Collections;
using UnityEngine;
using Framework.Event;
using YooAsset;

public sealed class GameManager
{
    private static GameManager s_instance;
    public static GameManager Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new GameManager();
            return s_instance;
        }
    }

    private EventSubscriptionScope _eventScope;
    private ResourcePackage _gamePackage;
    private MonoBehaviour _behaviour;

    /// <summary>
    /// Game package.
    /// </summary>
    public ResourcePackage GamePackage
    {
        get
        {
            if (_gamePackage == null)
                throw new InvalidOperationException("Game package has not been set. Call SetGamePackage before loading game assets.");
            return _gamePackage;
        }
    }

    /// <summary>
    /// Sets the game package.
    /// </summary>
    public void SetGamePackage(ResourcePackage gamePackage)
    {
        _gamePackage = gamePackage ?? throw new ArgumentNullException(nameof(gamePackage));
    }

    /// <summary>
    /// Sets the coroutine runner.
    /// </summary>
    public void SetBehaviour(MonoBehaviour behaviour)
    {
        _behaviour = behaviour ?? throw new ArgumentNullException(nameof(behaviour));
    }

    private GameManager()
    {
    }

    public void Create()
    {
        if (_eventScope != null)
            throw new InvalidOperationException("Game manager has already been created.");

        // Register event listeners.
        _eventScope = EventManager.Instance.CreateScope();
        _eventScope.AddListener<SceneChangeToHomeEvent>(OnHandleEventMessage);
        _eventScope.AddListener<SceneChangeToBattleEvent>(OnHandleEventMessage);
    }

    public void Destroy()
    {
        _eventScope?.Dispose();
        _eventScope = null;
        _gamePackage = null;
        _behaviour = null;
    }

    /// <summary>
    /// Starts a coroutine.
    /// </summary>
    public void StartCoroutine(IEnumerator enumerator)
    {
        if (enumerator == null)
            throw new ArgumentNullException(nameof(enumerator));
        if (_behaviour == null)
            throw new InvalidOperationException("Coroutine runner has not been set. Call SetBehaviour before starting coroutines.");

        _behaviour.StartCoroutine(enumerator);
    }

    /// <summary>
    /// Handles event messages.
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is SceneChangeToHomeEvent)
        {
            GamePackage.LoadSceneAsync("scene_home");
        }
        else if (message is SceneChangeToBattleEvent)
        {
            GamePackage.LoadSceneAsync("scene_battle");
        }
    }
}
