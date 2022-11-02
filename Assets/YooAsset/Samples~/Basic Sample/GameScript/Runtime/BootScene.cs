using System;
using System.Collections;
using UnityEngine;
using YooAsset;
using Better.StreamingAssets;
using System.IO;

public class BootScene : MonoBehaviour
{
	public static BootScene Instance { private set; get; }
	public static EPlayMode GamePlayMode;

	public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

	void Awake()
	{
		Instance = this;

		Application.targetFrameRate = 60;
		Application.runInBackground = true;
	}
	void OnGUI()
	{
		GUIConsole.OnGUI();
	}
	void OnDestroy()
	{
		Instance = null;
	}
	void Update()
	{
		EventManager.Update();
		FsmManager.Update();
	}

	IEnumerator Start()
	{
		GamePlayMode = PlayMode;
		Debug.Log($"资源系统运行模式：{PlayMode}");

		// 初始化BetterStreaming
		BetterStreamingAssets.Initialize();

		// 初始化资源系统
		YooAssets.Initialize();

		// 创建默认的资源包
		var defaultPackage = YooAssets.CreateAssetsPackage("DefaultPackage");

		// 设置该资源包为默认的资源包
		YooAssets.SetDefaultAssetsPackage(defaultPackage);

		// 编辑器下的模拟模式
		if (PlayMode == EPlayMode.EditorSimulateMode)
		{
			var createParameters = new EditorSimulateModeParameters();
			createParameters.SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
			yield return defaultPackage.InitializeAsync(createParameters);
		}

		// 单机运行模式
		if (PlayMode == EPlayMode.OfflinePlayMode)
		{
			var createParameters = new OfflinePlayModeParameters();
			createParameters.DecryptionServices = new BundleDecryptionServices();
			yield return defaultPackage.InitializeAsync(createParameters);
		}

		// 联机运行模式
		if (PlayMode == EPlayMode.HostPlayMode)
		{
			var createParameters = new HostPlayModeParameters();
			createParameters.DecryptionServices = new BundleDecryptionServices();
			createParameters.QueryServices = new QueryStreamingAssetsFileServices();
			createParameters.DefaultHostServer = GetHostServerURL();
			createParameters.FallbackHostServer = GetHostServerURL();
			yield return defaultPackage.InitializeAsync(createParameters);
		}

		// 运行补丁流程
		PatchUpdater.Run();
	}

	private string GetHostServerURL()
	{
		//string hostServerIP = "http://10.0.2.2"; //安卓模拟器地址
		string hostServerIP = "http://127.0.0.1";
		string gameVersion = "v1.0";

#if UNITY_EDITOR
		if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
			return $"{hostServerIP}/CDN/Android/{gameVersion}";
		else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
			return $"{hostServerIP}/CDN/IPhone/{gameVersion}";
		else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
			return $"{hostServerIP}/CDN/WebGL/{gameVersion}";
		else
			return $"{hostServerIP}/CDN/PC/{gameVersion}";
#else
		if (Application.platform == RuntimePlatform.Android)
			return $"{hostServerIP}/CDN/Android/{gameVersion}";
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
			return $"{hostServerIP}/CDN/IPhone/{gameVersion}";
		else if (Application.platform == RuntimePlatform.WebGLPlayer)
			return $"{hostServerIP}/CDN/WebGL/{gameVersion}";
		else
			return $"{hostServerIP}/CDN/PC/{gameVersion}";
#endif
	}
	private class QueryStreamingAssetsFileServices : IQueryServices
	{
		public bool QueryStreamingAssets(string fileName)
		{
			// 注意：使用了BetterStreamingAssets插件，使用前需要初始化该插件！
			string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
			return BetterStreamingAssets.FileExists($"{buildinFolderName}/{fileName}");
		}
	}
	private class BundleDecryptionServices : IDecryptionServices
	{
		public ulong LoadFromFileOffset(DecryptFileInfo fileInfo)
		{
			return 32;
		}

		public byte[] LoadFromMemory(DecryptFileInfo fileInfo)
		{
			throw new NotImplementedException();
		}

		public FileStream LoadFromStream(DecryptFileInfo fileInfo)
		{
			BundleStream bundleStream = new BundleStream(fileInfo.FilePath, FileMode.Open);
			return bundleStream;
		}

		public uint GetManagedReadBufferSize()
		{
			return 1024;
		}
	}
}