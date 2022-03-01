using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class DatabaseSceneProvider : AssetProviderBase
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

		public DatabaseSceneProvider(string scenePath, SceneInstanceParam param)
			: base(scenePath, null)
		{
			_param = param;
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (States == EAssetStates.None)
			{
				States = EAssetStates.Loading;
			}

			// 1. 加载资源对象
			if (States == EAssetStates.Loading)
			{
				LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
				loadSceneParameters.loadSceneMode = _param.LoadMode;			
				_asyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(AssetPath, loadSceneParameters);
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					States = EAssetStates.Checking;
				}
				else
				{
					Logger.Warning($"Failed to load scene : {AssetName}");
					States = EAssetStates.Fail;
					InvokeCompletion();
				}
			}

			// 2. 检测加载结果
			if (States == EAssetStates.Checking)
			{
				if (_asyncOp.isDone)
				{
					SceneInstance instance = new SceneInstance(_asyncOp);
					instance.Scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					AssetInstance = instance;
					if(_param.ActivateOnLoad)
						instance.Activate();

					States = instance.Scene.IsValid() ? EAssetStates.Success : EAssetStates.Fail;
					InvokeCompletion();
				}
			}
#endif
		}
		public override void Destory()
		{
#if UNITY_EDITOR
			base.Destory();

			// 卸载附加场景（异步方式卸载）
			if (_param.LoadMode == LoadSceneMode.Additive)
			{
				var instance = AssetInstance as SceneInstance;
				if(instance != null && instance.Scene != null)
				{
					if (instance.Scene.IsValid() && instance.Scene.isLoaded)
						SceneManager.UnloadSceneAsync(instance.Scene);
				}
			}
#endif
		}
		public override void WaitForAsyncComplete()
		{
			throw new System.Exception($"Unity scene is not support {nameof(WaitForAsyncComplete)}.");
		}
	}
}