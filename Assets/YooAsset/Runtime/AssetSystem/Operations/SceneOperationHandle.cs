using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	public class SceneOperationHandle : OperationHandleBase
	{
		/// <summary>
		/// 场景卸载异步操作类
		/// </summary>
		public class UnloadSceneOperation : AsyncOperationBase
		{
			private enum EFlag
			{
				Normal,
				Error,
				Skip,
			}
			private enum ESteps
			{
				None,
				UnLoad,
				Checking,
				Done,
			}

			private readonly EFlag _flag;
			private ESteps _steps = ESteps.None;
			private Scene _scene;
			private AsyncOperation _asyncOp;

			/// <summary>
			/// 场景卸载进度
			/// </summary>
			public float Progress
			{
				get
				{
					if (_asyncOp == null)
						return 0;
					return _asyncOp.progress;
				}
			}

			internal UnloadSceneOperation()
			{
				_flag = EFlag.Skip;
			}
			internal UnloadSceneOperation(string error)
			{
				_flag = EFlag.Error;
				Error = error;
			}
			internal UnloadSceneOperation(Scene scene)
			{
				_flag = EFlag.Normal;
				_scene = scene;
			}
			internal override void Start()
			{
				if (_flag == EFlag.Normal)
				{
					_steps = ESteps.UnLoad;
				}
				else if (_flag == EFlag.Skip)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else if (_flag == EFlag.Error)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
				}
				else
				{
					throw new System.NotImplementedException(_flag.ToString());
				}
			}
			internal override void Update()
			{
				if (_steps == ESteps.None || _steps == ESteps.Done)
					return;

				if (_steps == ESteps.UnLoad)
				{
					if (_scene.IsValid() && _scene.isLoaded)
					{
						_asyncOp = SceneManager.UnloadSceneAsync(_scene);
						_steps = ESteps.Checking;
					}
					else
					{
						Error = "Scene is invalid or is not loaded.";
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
					}
				}

				if (_steps == ESteps.Checking)
				{
					if (_asyncOp.isDone == false)
						return;

					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}
		}


		private System.Action<SceneOperationHandle> _callback;

		internal SceneOperationHandle(AssetProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			if (IsValid)
			{
				_callback?.Invoke(this);
			}
		}

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<SceneOperationHandle> Completed
		{
			add
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SceneOperationHandle)} is invalid");
				if (_provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SceneOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 场景对象
		/// </summary>
		public Scene SceneObject
		{
			get
			{
				if (IsValid == false)
					return new Scene();
				return _provider.SceneObject;
			}
		}

		/// <summary>
		/// 激活场景
		/// </summary>
		public bool ActivateScene()
		{
			if (IsValid == false)
				return false;

			if (SceneObject.IsValid() && SceneObject.isLoaded)
			{
				return SceneManager.SetActiveScene(SceneObject);
			}
			else
			{
				YooLogger.Warning($"Scene is invalid or not loaded : {SceneObject.name}");
				return false;
			}
		}

		/// <summary>
		/// 异步卸载场景
		/// </summary>
		public UnloadSceneOperation UnloadAsync()
		{
			if (IsValid == false)
			{
				string error = $"{nameof(SceneOperationHandle)} is invalid.";
				var operation = new UnloadSceneOperation(error);
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}

			AssetProviderBase provider = _provider;

			// 释放场景句柄
			ReleaseInternal();

			// 卸载未被使用的资源（包括场景）
			AssetSystem.UnloadUnusedAssets();

			// 返回场景卸载异步操作类
			if (provider.IsDestroyed == false)
			{
				YooLogger.Warning($"Scene can not unload. The provider not destroyed : {provider.AssetPath}");
				var operation = new UnloadSceneOperation();
				OperationSystem.ProcessOperaiton(operation);
				return operation;
			}
			else
			{
				if (IsAdditiveScene(provider))
				{
					var operation = new UnloadSceneOperation(provider.SceneObject);
					OperationSystem.ProcessOperaiton(operation);
					return operation;
				}
				else
				{
					var operation = new UnloadSceneOperation();
					OperationSystem.ProcessOperaiton(operation);
					return operation;
				}
			}
		}

		private bool IsAdditiveScene(AssetProviderBase provider)
		{
			if (provider is DatabaseSceneProvider)
			{
				var temp = provider as DatabaseSceneProvider;
				return temp.SceneMode == LoadSceneMode.Additive;
			}
			else if (provider is BundledSceneProvider)
			{
				var temp = provider as BundledSceneProvider;
				return temp.SceneMode == LoadSceneMode.Additive;
			}
			else
			{
				throw new System.NotImplementedException();
			}
		}
	}
}