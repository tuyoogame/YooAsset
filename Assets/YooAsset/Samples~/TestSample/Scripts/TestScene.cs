using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using YooAsset;

public class TestScene : MonoBehaviour
{
	public YooAssets.EPlayMode PlayMode = YooAssets.EPlayMode.EditorSimulateMode;

	void Awake()
	{
		Application.targetFrameRate = 60;
		Application.runInBackground = true;
	}
	void OnGUI()
	{
		GUIConsole.OnGUI();
	}

	IEnumerator Start()
	{
		Debug.Log($"资源系统运行模式：{PlayMode}");

		// 编辑器模拟模式
		if (PlayMode == YooAssets.EPlayMode.EditorSimulateMode)
		{
			var createParameters = new YooAssets.EditorSimulateModeParameters();
			createParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
			yield return YooAssets.InitializeAsync(createParameters);
		}

		// 单机模式
		if (PlayMode == YooAssets.EPlayMode.OfflinePlayMode)
		{
			var createParameters = new YooAssets.OfflinePlayModeParameters();
			createParameters.LocationServices = new DefaultLocationServices("Assets/GameRes");
			yield return YooAssets.InitializeAsync(createParameters);
		}

		// 联机模式
		if (PlayMode == YooAssets.EPlayMode.HostPlayMode)
		{
			throw new NotImplementedException();
		}

		// 开始测试
		BeginTest();
	}

	void BeginTest()
	{
		AutoTestLog("开启单元测试 !");

		// 开始同步测试
		SyncTest();
	}
	void OverTest()
	{
		AutoTestLog("结束单元测试 !");
	}
	void AutoTestLog(string info)
	{
		Debug.Log($"[{Time.frameCount}] {info}");
	}

	#region 同步测试
	void SyncTest()
	{
		SyncTest1();
		SyncTest2();

		// 开始回调测试
		CallbackTest();
	}
	void SyncTest1()
	{
		AutoTestLog($"开始同步加载游戏对象测试 !");
		var handle = YooAssets.LoadAssetSync<GameObject>("Entity/Cube/cube1");
		Debug.Assert(handle.Status == EOperationStatus.Succeed);
		var go = handle.InstantiateSync();
		Debug.Assert(go != null);
		GameObject.Destroy(go);
		handle.Release();
	}
	void SyncTest2()
	{
		AutoTestLog($"开始同步加载TexturePacker图集测试 !");
		var handle = YooAssets.LoadSubAssetsSync<Sprite>("UIAtlas/TexturePacker/tpAtlas");
		Debug.Assert(handle.Status == EOperationStatus.Succeed);
		var sprite = handle.GetSubAssetObject<Sprite>("Icon_Sword_128");
		Debug.Assert(sprite != null);
		handle.Release();
	}
	#endregion

	#region 回调测试
	void CallbackTest()
	{
		CallbackTest1();
	}
	void CallbackTest1()
	{
		AutoTestLog($"开始异步加载游戏对象，回调测试 !");
		var handle = YooAssets.LoadAssetAsync<GameObject>("Entity/Cube/cube2");
		handle.Completed += (h) =>
		{
			Debug.Assert(handle.Status == EOperationStatus.Succeed);
			var operation = handle.InstantiateAsync();
			operation.Completed += (o) =>
			{
				Debug.Assert(operation.Status == EOperationStatus.Succeed);
				Debug.Assert(operation.Result != null);
				GameObject.Destroy(operation.Result);
				handle.Release();
				CallbackTest2();
			};
		};
	}
	void CallbackTest2()
	{
		AutoTestLog($"开始异步加载原生文件，回调测试 !");
		var operation = YooAssets.GetRawFileAsync("Config/config2");
		operation.Completed += (o) =>
		{
			Debug.Assert(operation.Status == EOperationStatus.Succeed);

			// 开始协程测试
			this.StartCoroutine(CoroutineTest());
		};
	}
	#endregion

	#region 协程测试
	IEnumerator CoroutineTest()
	{
		yield return CoroutineTest1();
		yield return CoroutineTest2();

		//开始Task测试
		TaskTest();
	}
	IEnumerator CoroutineTest1()
	{
		AutoTestLog($"开始异步加载游戏对象，协程测试 !");
		var handle = YooAssets.LoadAssetAsync<GameObject>("Entity/Cube/cube3");
		yield return handle;
		Debug.Assert(handle.Status == EOperationStatus.Succeed);
		var operation = handle.InstantiateAsync();
		yield return operation;
		Debug.Assert(operation.Status == EOperationStatus.Succeed);
		Debug.Assert(operation.Result != null);
		GameObject.Destroy(operation.Result);
		handle.Release();
	}
	IEnumerator CoroutineTest2()
	{
		AutoTestLog($"开始异步加载原生文件，协程测试 !");
		var operation = YooAssets.GetRawFileAsync("Config/config3");
		yield return operation;
		Debug.Assert(operation.Status == EOperationStatus.Succeed);
		yield return operation;
		Debug.Assert(operation.Status == EOperationStatus.Succeed);
	}
	#endregion

	#region Task测试
	async void TaskTest()
	{
		await TaskTest1();
		await TaskTest2();

		// 开始错误测试
		ErrorTest();
	}
	async Task TaskTest1()
	{
		AutoTestLog($"开始异步加载游戏对象，Task测试 !");
		var handle = YooAssets.LoadAssetAsync<GameObject>("Entity/Cube/cube4");
		await handle.Task;
		Debug.Assert(handle.Status == EOperationStatus.Succeed);
		var operation = handle.InstantiateAsync();
		await operation.Task;
		Debug.Assert(operation.Status == EOperationStatus.Succeed);
		Debug.Assert(operation.Result != null);
		GameObject.Destroy(operation.Result);
		handle.Release();
	}
	async Task TaskTest2()
	{
		AutoTestLog($"开始异步加载原生文件，Task测试 !");
		var operation = YooAssets.GetRawFileAsync("Config/config4");
		await operation.Task;
		Debug.Assert(operation.Status == EOperationStatus.Succeed);
		await operation.Task;
		Debug.Assert(operation.Status == EOperationStatus.Succeed);
	}
	#endregion

	#region 错误测试
	void ErrorTest()
	{
		AutoTestLog($"开始错误加载的测试 !");

		var handle1 = YooAssets.LoadAssetSync<GameObject>("");
		Debug.Assert(handle1.Status == EOperationStatus.Failed);

		var handle2 = YooAssets.LoadAssetSync<GameObject>("xxx1");
		Debug.Assert(handle2.Status == EOperationStatus.Failed);

		var result = YooAssets.IsNeedDownloadFromRemote("xxx2");
		Debug.Assert(result == false);

		var operaiton = YooAssets.GetRawFileAsync("xxx3");
		Debug.Assert(operaiton.Status == EOperationStatus.Failed);

		// 结束测试
		OverTest();
	}
	#endregion
}