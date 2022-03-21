using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class BundledSceneProvider : BundledProvider
	{
		private readonly LoadSceneMode _sceneMode;
		private readonly bool _activateOnLoad;
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

		public BundledSceneProvider(string scenePath, LoadSceneMode sceneMode, bool activateOnLoad)
			: base(scenePath, null)
		{
			_sceneMode = sceneMode;
			_activateOnLoad = activateOnLoad;
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
				_asyncOp = SceneManager.LoadSceneAsync(AssetName, _sceneMode);
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
					Scene = SceneManager.GetSceneByName(AssetName);
					if (_activateOnLoad)
						SceneManager.SetActiveScene(Scene);

					Status = Scene.IsValid() ? EStatus.Success : EStatus.Fail;
					InvokeCompletion();
				}
			}
		}
		public override void Destory()
		{
			base.Destory();

			// 卸载附加场景（异步方式卸载）
			if (_sceneMode == LoadSceneMode.Additive)
			{
				if (Scene.IsValid() && Scene.isLoaded)
					SceneManager.UnloadSceneAsync(Scene);
			}
		}
	}
}