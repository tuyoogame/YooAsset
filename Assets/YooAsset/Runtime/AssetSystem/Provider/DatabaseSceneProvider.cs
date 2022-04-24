using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	internal sealed class DatabaseSceneProvider : ProviderBase
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

		public DatabaseSceneProvider(string scenePath, LoadSceneMode sceneMode, bool activateOnLoad, int priority)
			: base(scenePath, null)
		{
			SceneMode = sceneMode;
			_activateOnLoad = activateOnLoad;
			_priority = priority;
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
				loadSceneParameters.loadSceneMode = SceneMode;
				_asyncOp = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(AssetPath, loadSceneParameters);
				if (_asyncOp != null)
				{
					_asyncOp.allowSceneActivation = true;
					_asyncOp.priority = _priority;
					Status = EStatus.Checking;
				}
				else
				{
					Status = EStatus.Fail;
					LastError = $"Failed to load scene : {AssetPath}";
					YooLogger.Error(LastError);
					InvokeCompletion();
				}
			}

			// 2. 检测加载结果
			if (Status == EStatus.Checking)
			{
				if (_asyncOp.isDone)
				{
					SceneObject = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
					if (SceneObject.IsValid() && _activateOnLoad)
						SceneManager.SetActiveScene(SceneObject);

					Status = SceneObject.IsValid() ? EStatus.Success : EStatus.Fail;
					if (Status == EStatus.Fail)
					{
						LastError = $"The load scene is invalid : {AssetPath}";
						YooLogger.Error(LastError);
					}
					InvokeCompletion();
				}
			}
#endif
		}
	}
}