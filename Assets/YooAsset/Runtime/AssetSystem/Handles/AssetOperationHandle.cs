using UnityEngine;

namespace YooAsset
{
	public class AssetOperationHandle : OperationHandleBase
	{
		private System.Action<AssetOperationHandle> _callback;

		internal AssetOperationHandle(ProviderBase provider) : base(provider)
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
		public event System.Action<AssetOperationHandle> Completed
		{
			add
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(AssetOperationHandle)} is invalid");
				if (_provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(AssetOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 资源对象
		/// </summary>
		public UnityEngine.Object AssetObject
		{
			get
			{
				if (IsValid == false)
					return null;
				return _provider.AssetObject;
			}
		}

		/// <summary>
		/// 等待异步执行完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			if (IsValid == false)
				return;
			_provider.WaitForAsyncComplete();
		}

		/// <summary>
		/// 释放资源句柄
		/// </summary>
		public void Release()
		{
			this.ReleaseInternal();
		}


		/// <summary>
		/// 同步初始化游戏对象
		/// </summary>
		/// <param name="parent">父类对象</param>
		/// <returns></returns>
		public GameObject InstantiateSync(Transform parent = null)
		{
			return InstantiateSync(Vector3.zero, Quaternion.identity, parent);
		}

		/// <summary>
		/// 同步初始化游戏对象
		/// </summary>
		/// <param name="position">坐标</param>
		/// <param name="rotation">角度</param>
		/// <param name="parent">父类对象</param>
		public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			if (IsValid == false)
				return null;
			if (_provider.AssetObject == null)
				return null;

			if (parent == null)
				return UnityEngine.Object.Instantiate(_provider.AssetObject as GameObject, position, rotation);
			else
				return UnityEngine.Object.Instantiate(_provider.AssetObject as GameObject, position, rotation, parent);
		}

		/// <summary>
		/// 异步初始化游戏对象
		/// </summary>
		/// <param name="parent">父类对象</param>
		public InstantiateOperation InstantiateAsync(Transform parent = null)
		{
			return InstantiateAsync(Vector3.zero, Quaternion.identity, parent);
		}

		/// <summary>
		/// 异步初始化游戏对象
		/// </summary>
		/// <param name="position">坐标</param>
		/// <param name="rotation">角度</param>
		/// <param name="parent">父类对象</param>
		public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			InstantiateOperation operation = new InstantiateOperation(this, position, rotation, parent);
			OperationSystem.ProcessOperaiton(operation);
			return operation;
		}
	}
}