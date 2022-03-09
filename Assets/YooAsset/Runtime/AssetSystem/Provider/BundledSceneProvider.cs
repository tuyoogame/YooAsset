using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class BundledSceneProvider : BundledProvider
	{
		private SceneInstanceParam _param;
		private AsyncOperation _asyncOp;
		public override float Progress
		{
			get
			{
				if (_asyncOp == null)
					return 0;
				return _asyncOp.progress;
			}
		}

		public BundledSceneProvider(string scenePath, SceneInstanceParam param)
			: base(scenePath, null)
		{
			_param = param;
		}
		public override void Update()
		{
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				States = EAssetStates.CheckBundle;
			}

			// 1. 检测资源包
			if (States == EAssetStates.CheckBundle)
			{
				if (DependBundles.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.CacheBundle == null)
				{
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
				else
				{
					States = EAssetStates.Loading;
				}
			}

			// 2. 加载场景
			if (States == EAssetStates.Loading)
			{
				_asyncOp = SceneManager.LoadSceneAsync(AssetName, _param.LoadMode);		
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					States = EAssetStates.Checking;
				}
				else
				{
					YooLogger.Warning($"Failed to load scene : {AssetName}");
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
			}

			// 3. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				if (_asyncOp.isDone)
				{
					SceneInstance instance = new SceneInstance(_asyncOp);
					instance.Scene = SceneManager.GetSceneByName(AssetName);
					AssetInstance = instance;
					if (_param.ActivateOnLoad)
						instance.Activate();

					States = instance.Scene.IsValid() ? EAssetStates.Success : EAssetStates.Fail;
					InvokeCompletion();
				}
			}
		}
		public override void Destory()
		{
			base.Destory();

			// 卸载附加场景（异步方式卸载）
			if (_param.LoadMode == LoadSceneMode.Additive)
			{
				var instance = AssetInstance as SceneInstance;
				if (instance != null && instance.Scene != null)
				{
					if (instance.Scene.IsValid() && instance.Scene.isLoaded)
						SceneManager.UnloadSceneAsync(instance.Scene);
				}
			}
		}
		public override void WaitForAsyncComplete()
		{
			throw new System.Exception($"Unity scene is not support {nameof(WaitForAsyncComplete)}.");
		}
	}
}