using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class BundledSceneProvider : BundledProvider
	{
		public readonly LoadSceneMode SceneMode;
		private readonly bool _activateOnLoad;
		private readonly int _priority;
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

		public BundledSceneProvider(string scenePath, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
			: base(scenePath, null)
		{
			SceneMode = sceneMode;
			_activateOnLoad = activateOnLoad;
			_priority = priority;
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
				_asyncOp = SceneManager.LoadSceneAsync(AssetName, SceneMode);
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					_asyncOp.priority = _priority;
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
					SceneObject = SceneManager.GetSceneByName(AssetName);
					if (SceneObject.IsValid() && _activateOnLoad)
						SceneManager.SetActiveScene(SceneObject);

					Status = SceneObject.IsValid() ? EStatus.Success : EStatus.Fail;
					InvokeCompletion();
				}
			}
		}
	}
}