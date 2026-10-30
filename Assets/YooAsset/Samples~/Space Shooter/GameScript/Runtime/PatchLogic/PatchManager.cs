using System;
using Framework.Machine;
using Framework.Event;
using YooAsset;

public sealed class PatchManager
{
    private static PatchManager s_instance;
    public static PatchManager Instance
    {
        get
        {
            if (s_instance == null)
                s_instance = new PatchManager();
            return s_instance;
        }
    }

    private EventSubscriptionScope _eventScope;
    private FlowMachine<PatchState, PatchContext> _machine;

    private PatchManager()
    {
    }

    public void Create(string packageName, EPlayMode playMode)
    {
        if (string.IsNullOrWhiteSpace(packageName))
            throw new ArgumentException("Package name cannot be null or empty.", nameof(packageName));
        if (!IsValidPlayMode(playMode))
            throw new ArgumentException($"Invalid play mode: {playMode}.", nameof(playMode));

        // Register event listeners.
        _eventScope?.Dispose();
        _eventScope = EventManager.Instance.CreateScope();
        _eventScope.AddListener<UserTryInitializePackageEvent>(OnHandleEventMessage);
        _eventScope.AddListener<UserBeginDownloadWebFilesEvent>(OnHandleEventMessage);
        _eventScope.AddListener<UserTryRequestPackageVersionEvent>(OnHandleEventMessage);
        _eventScope.AddListener<UserTryUpdatePackageManifestEvent>(OnHandleEventMessage);
        _eventScope.AddListener<UserTryDownloadWebFilesEvent>(OnHandleEventMessage);

        // Create state machine.
        _machine?.Stop();
        var context = new PatchContext(packageName, playMode);
        _machine = new FlowMachineBuilder<PatchState, PatchContext>()
            .AddState<FsmInitializePackage>(PatchState.InitializePackage)
            .AddState<FsmRequestPackageVersion>(PatchState.RequestPackageVersion)
            .AddState<FsmUpdatePackageManifest>(PatchState.UpdatePackageManifest)
            .AddState<FsmCreateDownloader>(PatchState.CreateDownloader)
            .AddState<FsmDownloadPackageFiles>(PatchState.DownloadPackageFiles)
            .AddState<FsmDownloadPackageOver>(PatchState.DownloadPackageOver)
            .AddState<FsmClearCacheBundle>(PatchState.ClearCacheBundle)
            .AddState<FsmStartGame>(PatchState.StartGame)
            .SetStartState(PatchState.InitializePackage)
            .Build(context);
        context.Machine = _machine;
    }
    public void Start()
    {
        if (_machine == null)
            throw new InvalidOperationException("Patch manager has not been created. Call Create before Start.");

        _machine.Start();
    }
    public void Update()
    {
        if (_machine == null)
            throw new InvalidOperationException("Patch manager has not been created. Call Create before Update.");

        _machine.Update();
    }

    public void Destroy()
    {
        _eventScope?.Dispose();
        _eventScope = null;
        _machine?.Stop();
        _machine = null;
    }

    /// <summary>
    /// Handles event messages.
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is UserTryInitializePackageEvent)
        {
            _machine.RequestChange(PatchState.InitializePackage);
        }
        else if (message is UserBeginDownloadWebFilesEvent)
        {
            _machine.RequestChange(PatchState.DownloadPackageFiles);
        }
        else if (message is UserTryRequestPackageVersionEvent)
        {
            _machine.RequestChange(PatchState.RequestPackageVersion);
        }
        else if (message is UserTryUpdatePackageManifestEvent)
        {
            _machine.RequestChange(PatchState.UpdatePackageManifest);
        }
        else if (message is UserTryDownloadWebFilesEvent)
        {
            _machine.RequestChange(PatchState.CreateDownloader);
        }
    }

    private bool IsValidPlayMode(EPlayMode playMode)
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
