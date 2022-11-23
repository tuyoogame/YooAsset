using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public static class PatchManager
{
	private static bool _isRun = false;

	/// <summary>
	/// 下载器
	/// </summary>
	public static PatchDownloaderOperation Downloader { set; get; }

	/// <summary>
	/// 包裹的版本信息
	/// </summary>
	public static string PackageVersion { set; get; }

	/// <summary>
	/// 开启初始化流程
	/// </summary>
	public static void Run()
	{
		if (_isRun == false)
		{
			_isRun = true;

			Debug.Log("开启补丁更新流程...");

			// 注意：按照先后顺序添加流程节点
			FsmManager.AddNode(new FsmPatchInit());
			FsmManager.AddNode(new FsmUpdateVersion());
			FsmManager.AddNode(new FsmUpdateManifest());
			FsmManager.AddNode(new FsmCreateDownloader());
			FsmManager.AddNode(new FsmDownloadFiles());
			FsmManager.AddNode(new FsmPatchDone());
			FsmManager.AddNode(new FsmClearCache());
			FsmManager.AddNode(new FsmStartGame());
			FsmManager.Run(nameof(FsmPatchInit));
		}
		else
		{
			Debug.LogWarning("补丁更新已经正在进行中!");
		}
	}

	/// <summary>
	/// 处理请求操作
	/// </summary>
	public static void HandleOperation(EPatchOperation operation)
	{
		if (operation == EPatchOperation.BeginDownloadWebFiles)
		{
			FsmManager.Transition(nameof(FsmDownloadFiles));
		}
		else if(operation == EPatchOperation.TryUpdateStaticVersion)
		{
			FsmManager.Transition(nameof(FsmUpdateVersion));
		}
		else if (operation == EPatchOperation.TryUpdatePatchManifest)
		{
			FsmManager.Transition(nameof(FsmUpdateManifest));
		}
		else if (operation == EPatchOperation.TryDownloadWebFiles)
		{
			FsmManager.Transition(nameof(FsmCreateDownloader));
		}
		else
		{
			throw new NotImplementedException($"{operation}");
		}
	}
}