using UnityEngine;
using System.Collections.Generic;

namespace YooAsset
{
	public sealed class AssetOperationHandle : OperationHandleBase
	{
		private System.Action<AssetOperationHandle> _callback;

		internal AssetOperationHandle(ProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			_callback?.Invoke(this);
		}

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<AssetOperationHandle> Completed
		{
			add
			{
				if (IsValidWithWarning == false)
					throw new System.Exception($"{nameof(AssetOperationHandle)} is invalid");
				if (Provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValidWithWarning == false)
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
				if (IsValidWithWarning == false)
					return null;
				return Provider.AssetObject;
			}
		}

		/// <summary>
		/// 获取资源对象
		/// </summary>
		/// <typeparam name="TAsset">资源类型</typeparam>
		public TAsset GetAssetObject<TAsset>() where TAsset : UnityEngine.Object
		{
			if (IsValidWithWarning == false)
				return null;
			return Provider.AssetObject as TAsset;
		}

		/// <summary>
		/// 等待异步执行完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			if (IsValidWithWarning == false)
				return;
			Provider.WaitForAsyncComplete();
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
			return InstantiateSyncInternal(Vector3.zero, Quaternion.identity, parent);
		}

		/// <summary>
		/// 同步初始化游戏对象
		/// </summary>
		/// <param name="position">坐标</param>
		/// <param name="rotation">角度</param>
		/// <param name="parent">父类对象</param>
		public GameObject InstantiateSync(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			return InstantiateSyncInternal(position, rotation, parent);
		}

		/// <summary>
		/// 异步初始化游戏对象
		/// </summary>
		/// <param name="parent">父类对象</param>
		public InstantiateOperation InstantiateAsync(Transform parent = null)
		{
			return InstantiateAsyncInternal(Vector3.zero, Quaternion.identity, parent);
		}

		/// <summary>
		/// 异步初始化游戏对象
		/// </summary>
		/// <param name="position">坐标</param>
		/// <param name="rotation">角度</param>
		/// <param name="parent">父类对象</param>
		public InstantiateOperation InstantiateAsync(Vector3 position, Quaternion rotation, Transform parent = null)
		{
			return InstantiateAsyncInternal(position, rotation, parent);
		}


		private GameObject InstantiateSyncInternal(Vector3 position, Quaternion rotation, Transform parent)
		{
			if (IsValidWithWarning == false)
				return null;
			if (Provider.AssetObject == null)
				return null;

			GameObject clone = UnityEngine.Object.Instantiate(Provider.AssetObject as GameObject, position, rotation, parent);			
			return clone;
		}
		private InstantiateOperation InstantiateAsyncInternal(Vector3 position, Quaternion rotation, Transform parent)
		{
			InstantiateOperation operation = new InstantiateOperation(this, position, rotation, parent);
			OperationSystem.StartOperation(operation);
			return operation;
		}
	}
}
