using UnityEngine.SceneManagement;

namespace YooAsset
{
	public class SceneOperationHandle : OperationHandleBase
	{
		private System.Action<SceneOperationHandle> _callback;

		internal SceneOperationHandle(ProviderBase provider) : base(provider)
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

			ProviderBase provider = _provider;

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

		private bool IsAdditiveScene(ProviderBase provider)
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