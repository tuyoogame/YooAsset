using System;
using UniFramework.Machine;
using UniFramework.Event;
using YooAsset;

public static class PatchManager
{
    private static readonly EventGroup _eventGroup = new EventGroup();
    private static StateMachine _machine;

    public static void Create(string packageName, EPlayMode playMode)
    {
        if (string.IsNullOrWhiteSpace(packageName))
            throw new ArgumentException("Package name cannot be null or empty.", nameof(packageName));
        if (!IsValidPlayMode(playMode))
            throw new ArgumentException($"Invalid play mode: {playMode}.", nameof(playMode));

        // Register event listeners.
        _eventGroup.AddListener<UserTryInitializePackageEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<UserBeginDownloadWebFilesEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<UserTryRequestPackageVersionEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<UserTryUpdatePackageManifestEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<UserTryDownloadWebFilesEvent>(OnHandleEventMessage);

        // Create state machine.
        _machine = new StateMachine(null);
        _machine.AddNode<FsmInitializePackage>();
        _machine.AddNode<FsmRequestPackageVersion>();
        _machine.AddNode<FsmUpdatePackageManifest>();
        _machine.AddNode<FsmCreateDownloader>();
        _machine.AddNode<FsmDownloadPackageFiles>();
        _machine.AddNode<FsmDownloadPackageOver>();
        _machine.AddNode<FsmClearCacheBundle>();
        _machine.AddNode<FsmStartGame>();

        _machine.SetBlackboardValue("PackageName", packageName);
        _machine.SetBlackboardValue("PlayMode", playMode);
    }
    public static void Start()
    {
        if (_machine == null)
            throw new InvalidOperationException("Patch manager has not been created. Call Create before Start.");

        _machine.Run<FsmInitializePackage>();
    }
    public static void Update()
    {
        if (_machine == null)
            throw new InvalidOperationException("Patch manager has not been created. Call Create before Update.");

        _machine.Update();
    }

    /// <summary>
    /// Handles event messages.
    /// </summary>
    private static void OnHandleEventMessage(IEventMessage message)
    {
        if (message is UserTryInitializePackageEvent)
        {
            _machine.ChangeState<FsmInitializePackage>();
        }
        else if (message is UserBeginDownloadWebFilesEvent)
        {
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
        else if (message is UserTryRequestPackageVersionEvent)
        {
            _machine.ChangeState<FsmRequestPackageVersion>();
        }
        else if (message is UserTryUpdatePackageManifestEvent)
        {
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
        else if (message is UserTryDownloadWebFilesEvent)
        {
            _machine.ChangeState<FsmCreateDownloader>();
        }
        else
        {
            throw new InvalidOperationException($"Unsupported patch event message type: {message.GetType().FullName}.");
        }
    }

    private static bool IsValidPlayMode(EPlayMode playMode)
    {
        switch (playMode)
        {
            case EPlayMode.EditorSimulateMode:
            case EPlayMode.OfflinePlayMode:
            case EPlayMode.HostPlayMode:
            case EPlayMode.WebPlayMode:
                return true;
            default:
                return false;
        }
    }
}