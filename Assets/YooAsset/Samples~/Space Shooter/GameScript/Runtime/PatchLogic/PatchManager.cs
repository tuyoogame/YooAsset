using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Event;
using UniFramework.Module;
using YooAsset;

public class PatchManager : ModuleSingleton<PatchManager>, IModule
{
	/// <summary>
	/// 运行模式
	/// </summary>
	public EPlayMode PlayMode { private set; get; }

	/// <summary>
	/// 包裹的版本信息
	/// </summary>
	public string PackageVersion { set; get; }

	/// <summary>
	/// 下载器
	/// </summary>
	public PatchDownloaderOperation Downloader { set; get; }


	private bool _isRun = false;
	private EventGroup _eventGroup = new EventGroup();
	private StateMachine _machine;

	void IModule.OnCreate(object createParam)
	{
	}
	void IModule.OnDestroy()
	{
		_eventGroup.RemoveAllListener();
	}
	void IModule.OnUpdate()
	{
		if (_machine != null)
			_machine.Update();
	}

	/// <summary>
	/// 开启流程
	/// </summary>
	public void Run(EPlayMode playMode)
	{
		if (_isRun == false)
		{
			_isRun = true;
			PlayMode = playMode;

			// 注册监听事件
			_eventGroup.AddListener<UserEventDefine.UserBeginDownloadWebFiles>(OnHandleEventMessage);
			_eventGroup.AddListener<UserEventDefine.UserTryUpdatePackageVersion>(OnHandleEventMessage);
			_eventGroup.AddListener<UserEventDefine.UserTryUpdatePatchManifest>(OnHandleEventMessage);
			_eventGroup.AddListener<UserEventDefine.UserTryDownloadWebFiles>(OnHandleEventMessage);

			Debug.Log("开启补丁更新流程...");
			_machine = new StateMachine(this);
			_machine.AddNode<FsmPatchInit>();
			_machine.AddNode<FsmUpdateVersion>();
			_machine.AddNode<FsmUpdateManifest>();
			_machine.AddNode<FsmCreateDownloader>();
			_machine.AddNode<FsmDownloadFiles>();
			_machine.AddNode<FsmDownloadOver>();	
			_machine.AddNode<FsmClearCache>();
			_machine.AddNode<FsmPatchDone>();
			_machine.Run<FsmPatchInit>();
		}
		else
		{
			Debug.LogWarning("补丁更新已经正在进行中!");
		}
	}

	/// <summary>
	/// 接收事件
	/// </summary>
	private void OnHandleEventMessage(IEventMessage message)
	{
		if (message is UserEventDefine.UserBeginDownloadWebFiles)
		{
			_machine.ChangeState<FsmDownloadFiles>();
		}
		else if (message is UserEventDefine.UserTryUpdatePackageVersion) 
		{
			_machine.ChangeState<FsmUpdateVersion>(); 
		}
		else if (message is UserEventDefine.UserTryUpdatePatchManifest)
		{
			_machine.ChangeState<FsmUpdateManifest>();
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