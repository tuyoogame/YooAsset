using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class DatabaseSceneProvider : AssetProviderBase
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

		public DatabaseSceneProvider(string scenePath, LoadSceneMode sceneMode, bool activateOnLoad)
			: base(scenePath, null)
		{
			_sceneMode = sceneMode;
			_activateOnLoad = activateOnLoad;
		}
		public override void Update()
		{
#if UNITY_EDITOR
			if (IsDone)
				return;

			if (Status == EStatus.None)
			{
				Status = EStatus.Loading;
			}

			// 1. 加载资源对象
			if (Status == EStatus.Loading)
			{
				LoadSceneParameters loadSceneParameters = new LoadSceneParameters();
				loadSceneParameters.loadSceneMode = _sceneMode;
				_asyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(AssetPath, loadSceneParameters);
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

			// 2. 检测加载结果
			if (Status == EStatus.Checking)
			{
				if (_asyncOp.isDone)
				{
					Scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					if (_activateOnLoad)
						SceneManager.SetActiveScene(Scene);

					Status = Scene.IsValid() ? EStatus.Success : EStatus.Fail;
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
			if (_sceneMode == LoadSceneMode.Additive)
			{
				if (Scene.IsValid() && Scene.isLoaded)
					SceneManager.UnloadSceneAsync(Scene);
			}
#endif
		}
	}
}