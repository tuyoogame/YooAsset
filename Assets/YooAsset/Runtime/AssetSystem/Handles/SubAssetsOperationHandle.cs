
namespace YooAsset
{
	public sealed class SubAssetsOperationHandle : OperationHandleBase
	{
		private System.Action<SubAssetsOperationHandle> _callback;

		internal SubAssetsOperationHandle(ProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			_callback?.Invoke(this);
		}

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<SubAssetsOperationHandle> Completed
		{
			add
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SubAssetsOperationHandle)} is invalid");
				if (Provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SubAssetsOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 子资源对象集合
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


		/// <summary>
		/// 获取子资源对象
		/// </summary>
		/// <typeparam name="TObject">子资源对象类型</typeparam>
		/// <param name="assetName">子资源对象名称</param>
		public TObject GetSubAssetObject<TObject>(string assetName) where TObject : UnityEngine.Object
		{
			if (IsValid == false)
				return null;

			foreach (var asset in Provider.AllAssetObjects)
			{
				if (asset.name == assetName)
					return asset as TObject;
			}

			YooLogger.Warning($"Not found sub asset object : {assetName}");
			return null;
		}
	}
}