
namespace YooAsset
{
	public sealed class AllAssetsOperationHandle : OperationHandleBase
	{
		private System.Action<AllAssetsOperationHandle> _callback;
		
		internal AllAssetsOperationHandle(ProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			_callback?.Invoke(this);
		}
		
		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<AllAssetsOperationHandle> Completed
		{
			add
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(AllAssetsOperationHandle)} is invalid");
				if (Provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(AllAssetsOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 资源包内的资源对象集合
		/// </summary>
		public UnityEngine.Object[] AllAssetObjects
		{
			get
			{
				if (IsValid == false)
					return null;
				return Provider.AllAssetObjects;
			}
		}

		/// <summary>
		/// 等待异步执行完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			if (IsValid == false)
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
	}
}