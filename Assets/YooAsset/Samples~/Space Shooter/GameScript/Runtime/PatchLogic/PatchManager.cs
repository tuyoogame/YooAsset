using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;
using YooAsset;

public static class PatchManager
{
    private static readonly EventGroup _eventGroup = new EventGroup();
    private static StateMachine _machine;

    public static void Create(string packageName, EPlayMode playMode)
    {
        // 注册监听事件
        _eventGroup.AddListener<UserEventDefine.UserTryInitialize>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserBeginDownloadWebFiles>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryRequestPackageVersion>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryUpdatePackageManifest>(OnHandleEventMessage);
        _eventGroup.AddListener<UserEventDefine.UserTryDownloadWebFiles>(OnHandleEventMessage);

        // 创建状态机
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
        _machine.Run<FsmInitializePackage>();
    }
    public static void Update()
    {
        _machine.Update();
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private static void OnHandleEventMessage(IEventMessage message)
    {
        if (message is UserEventDefine.UserTryInitialize)
        {
            _machine.ChangeState<FsmInitializePackage>();
        }
        else if (message is UserEventDefine.UserBeginDownloadWebFiles)
        {
            _machine.ChangeState<FsmDownloadPackageFiles>();
        }
        else if (message is UserEventDefine.UserTryRequestPackageVersion)
        {
            _machine.ChangeState<FsmRequestPackageVersion>();
        }
        else if (message is UserEventDefine.UserTryUpdatePackageManifest)
        {
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
        else if (message is UserEventDefine.UserTryDownloadWebFiles)
        {
            _machine.ChangeState<FsmCreateDownloader>();
        }
        else
        {
            throw new System.NotImplementedException($"{message.GetType()}");
        }
    }
}