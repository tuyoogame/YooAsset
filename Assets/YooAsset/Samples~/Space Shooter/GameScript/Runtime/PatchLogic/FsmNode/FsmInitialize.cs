using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Machine;
using UniFramework.Singleton;
using YooAsset;

/// <summary>
/// 初始化资源包
/// </summary>
internal class FsmInitialize : IStateNode
{
	private StateMachine _machine;

	void IStateNode.OnCreate(StateMachine machine)
	{
		_machine = machine;
	}
	void IStateNode.OnEnter()
	{
		PatchEventDefine.PatchStatesChange.SendEventMessage("初始化资源包！");
		UniSingleton.StartCoroutine(InitPackage());
	}
	void IStateNode.OnUpdate()
	{
	}
	void IStateNode.OnExit()
	{
	}

	private IEnumerator InitPackage()
	{
		yield return new WaitForSeconds(1f);

		var playMode = PatchManager.Instance.PlayMode;

		// 创建默认的资源包
		string packageName = "DefaultPackage";
		var package = YooAssets.TryGetPackage(packageName);
		if (package == null)
		{
			package = YooAssets.CreatePackage(packageName);
			YooAssets.SetDefaultPackage(package);
		}

		// 编辑器下的模拟模式
		InitializationOperation initializationOperation = null;
		if (playMode == EPlayMode.EditorSimulateMode)
		{
			var createParameters = new EditorSimulateModeParameters();
			createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(packageName);
			initializationOperation = package.InitializeAsync(createParameters);
		}

		// 单机运行模式
		if (playMode == EPlayMode.OfflinePlayMode)
		{
			var createParameters = new OfflinePlayModeParameters();
			createParameters.DecryptionServices = new GameDecryptionServices();
			initializationOperation = package.InitializeAsync(createParameters);
		}

		// 联机运行模式
		if (playMode == EPlayMode.HostPlayMode)
		{
			string defaultHostServer = GetHostServerURL();
			string fallbackHostServer = GetHostServerURL();
			var createParameters = new HostPlayModeParameters();
			createParameters.DecryptionServices = new GameDecryptionServices();
			createParameters.QueryServices = new GameQueryServices();
			createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
			initializationOperation = package.InitializeAsync(createParameters);
		}

		// WebGL运行模式
		if(playMode == EPlayMode.WebPlayMode)
		{
			string defaultHostServer = GetHostServerURL();
			string fallbackHostServer = GetHostServerURL();
			var createParameters = new WebPlayModeParameters();
			createParameters.DecryptionServices = new GameDecryptionServices();
			createParameters.QueryServices = new GameQueryServices();
			createParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
			initializationOperation = package.InitializeAsync(createParameters);
		}

		yield return initializationOperation;
		if (initializationOperation.Status == EOperationStatus.Succeed)
		{
			_machine.ChangeState<FsmUpdateVersion>();
		}
		else
		{
			Debug.LogWarning($"{initializationOperation.Error}");
			PatchEventDefine.InitializeFailed.SendEventMessage();
		}
	}

	/// <summary>
	/// 获取资源服务器地址
	/// </summary>
	private string GetHostServerURL()
	{
		//string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
		string hostServerIP = "http://127.0.0.1";
		string appVersion = "v1.0";

#if UNITY_EDITOR
		if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
			return $"{hostServerIP}/CDN/Android/{appVersion}";
		else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
			return $"{hostServerIP}/CDN/IPhone/{appVersion}";
		else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
			return $"{hostServerIP}/CDN/WebGL/{appVersion}";
		else
			return $"{hostServerIP}/CDN/PC/{appVersion}";
#else
		if (Application.platform == RuntimePlatform.Android)
			return $"{hostServerIP}/CDN/Android/{appVersion}";
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
			return $"{hostServerIP}/CDN/IPhone/{appVersion}";
		else if (Application.platform == RuntimePlatform.WebGLPlayer)
			return $"{hostServerIP}/CDN/WebGL/{appVersion}";
		else
			return $"{hostServerIP}/CDN/PC/{appVersion}";
#endif
	}


	/// <summary>
	/// 远端资源地址查询服务类
	/// </summary>
	private class RemoteServices : IRemoteServices
	{
		private readonly string _defaultHostServer;
		private readonly string _fallbackHostServer;

		public RemoteServices(string defaultHostServer, string fallbackHostServer)
		{
			_defaultHostServer = defaultHostServer;
			_fallbackHostServer = fallbackHostServer;
		}
		string IRemoteServices.GetRemoteMainURL(string fileName)
		{
			return $"{_defaultHostServer}/{fileName}";
		}
		string IRemoteServices.GetRemoteFallbackURL(string fileName)
		{
			return $"{_fallbackHostServer}/{fileName}";
		}
	}

	/// <summary>
	/// 资源文件解密服务类
	/// </summary>
	private class GameDecryptionServices : IDecryptionServices
	{
		public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
		{
			return 32;
		}

		public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
		{
			throw new NotImplementedException();
		}

		public Stream LoadFromStream(DecryptFileInfo fileInfo)
		{
			BundleStream bundleStream = new BundleStream(fileInfo.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			return bundleStream;
		}

		public uint GetManagedReadBufferSize()
		{
			return 1024;
		}
	}
}