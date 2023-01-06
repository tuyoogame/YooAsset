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
		Application.runInBackground = true;  		//TODO wht real 加入框架
	}
	void Start()
	{
		// 初始化BetterStreaming
		BetterStreamingAssets.Initialize();		//TODO wht ref 不是必要的

		// 初始化事件系统
		UniEvent.Initalize();		//TODO wht ref 不是必要的

		// 初始化管理系统
		UniModule.Initialize();		//TODO wht ref 不是必要的

		// 初始化资源系统
		YooAssets.Initialize();		//TODO wht real 拿
		YooAssets.SetOperationSystemMaxTimeSlice(30);		//TODO wht real 拿

		// TODO wht ref 上面 都要有

		// 创建补丁管理器
		UniModule.CreateModule<PatchManager>();		//TODO wht ref 不是必要的

		// 开始补丁更新流程
		PatchManager.Instance.Run(PlayMode);		//TODo wht ref 不是必要的

		//TODO wht real 把UniFramework和BetterStreamingAssets去掉
	}
}