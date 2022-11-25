using UnityEngine;
using UniFramework.Event;
using UniFramework.Module;
using YooAsset;

public class Boot : MonoBehaviour
{
	/// <summary>
	/// 资源系统运行模式
	/// </summary>
	public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

	void Awake()
	{
		Debug.Log($"资源系统运行模式：{PlayMode}");
		Application.targetFrameRate = 60;
		Application.runInBackground = true;
	}
	void Start()
	{
		// 初始化BetterStreaming
		BetterStreamingAssets.Initialize();

		// 初始化事件系统
		UniEvent.Initalize();

		// 初始化管理系统
		UniModule.Initialize();

		// 初始化资源系统
		YooAssets.Initialize();

		// 创建补丁管理器
		UniModule.CreateModule<PatchManager>();

		// 开始补丁更新流程
		PatchManager.Instance.Run(PlayMode);
	}
}