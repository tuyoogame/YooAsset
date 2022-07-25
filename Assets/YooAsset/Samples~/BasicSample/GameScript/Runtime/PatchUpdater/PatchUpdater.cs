using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public static class PatchUpdater
{
	private static bool _isRun = false;

	/// <summary>
	/// 下载器
	/// </summary>
	public static PatchDownloaderOperation Downloader { set; get; }

	/// <summary>
	/// 资源版本
	/// </summary>
	public static int ResourceVersion { set; get; }

	/// <summary>
	/// 开启初始化流程
	/// </summary>
	public static void Run()
	{
		if (_isRun == false)
		{
			_isRun = true;

			Debug.Log("开始补丁更新...");

			// 注意：按照先后顺序添加流程节点
			FsmManager.AddNode(new FsmPatchInit());
			FsmManager.AddNode(new FsmUpdateStaticVersion());
			FsmManager.AddNode(new FsmUpdateManifest());
			FsmManager.AddNode(new FsmCreateDownloader());
			FsmManager.AddNode(new FsmDownloadWebFiles());
			FsmManager.AddNode(new FsmPatchDone());
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
			FsmManager.Transition(nameof(FsmDownloadWebFiles));
		}
		else if(operation == EPatchOperation.TryUpdateStaticVersion)
		{
			FsmManager.Transition(nameof(FsmUpdateStaticVersion));
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