using System;
using System.Collections;
using UnityEngine;
using YooAsset;
using Better.StreamingAssets;

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
			createParameters.LocationServices = new AddressLocationServices();
			createParameters.SimulatePatchManifestPath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage", true);
			yield return defaultPackage.InitializeAsync(createParameters);
		}

		// 单机运行模式
		if (PlayMode == EPlayMode.OfflinePlayMode)
		{
			var createParameters = new OfflinePlayModeParameters();
			createParameters.LocationServices = new AddressLocationServices();
			yield return defaultPackage.InitializeAsync(createParameters);
		}

		// 联机运行模式
		if (PlayMode == EPlayMode.HostPlayMode)
		{
			var createParameters = new HostPlayModeParameters();
			createParameters.LocationServices = new AddressLocationServices();
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
			return BetterStreamingAssets.FileExists($"YooAssets/{fileName}");
		}
	}
}