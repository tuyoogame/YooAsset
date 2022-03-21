using UnityEngine;

namespace YooAsset
{
	public class AssetOperationHandle : OperationHandleBase
	{
		private System.Action<AssetOperationHandle> _callback;

		internal AssetOperationHandle(AssetProviderBase provider) : base(provider)
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
		/// 初始化的游戏对象（只限于请求的资源对象类型为GameObject）
		/// </summary>
		public GameObject InstantiateObject
		{
			get
			{
				if (IsValid == false)
					return null;
				if (_provider.AssetObject == null)
					return null;
				return UnityEngine.Object.Instantiate(_provider.AssetObject as GameObject);
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
	}
}