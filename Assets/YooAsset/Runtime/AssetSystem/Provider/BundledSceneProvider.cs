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

			if (Status == EStatus.None)
			{
				Status = EStatus.CheckBundle;
			}

			// 1. 检测资源包
			if (Status == EStatus.CheckBundle)
			{
				if (DependBundles.IsDone() == false)
					return;
				if (OwnerBundle.IsDone() == false)
					return;

				if (OwnerBundle.CacheBundle == null)
				{
					Status = EStatus.Fail;
					InvokeCompletion();
				}
				else
				{
					Status = EStatus.Loading;
				}
			}

			// 2. 加载场景
			if (Status == EStatus.Loading)
			{
				_asyncOp = SceneManager.LoadSceneAsync(AssetName, _param.LoadMode);		
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					Status = EStatus.Checking;
				}
				else
				{
					YooLogger.Warning($"Failed to load scene : {AssetName}");
					Status = EStatus.Fail;
					InvokeCompletion();
				}
			}

			// 3. 检测加载结果
			if (Status == EStatus.Checking)
			{
				if (_asyncOp.isDone)
				{
					SceneInstance instance = new SceneInstance(_asyncOp);
					instance.Scene = SceneManager.GetSceneByName(AssetName);
					AssetInstance = instance;
					if (_param.ActivateOnLoad)
						instance.Activate();

					Status = instance.Scene.IsValid() ? EStatus.Success : EStatus.Fail;
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