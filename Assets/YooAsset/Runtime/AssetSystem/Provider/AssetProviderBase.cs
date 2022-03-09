
namespace YooAsset
{
	internal abstract class AssetProviderBase : IAssetProvider
	{
		protected bool IsWaitForAsyncComplete { private set; get; } = false;
		
		public string AssetPath { private set; get; }
		public string AssetName { private set; get; }
		public System.Type AssetType { private set; get; }
		public UnityEngine.Object AssetObject { protected set; get; }
		public UnityEngine.Object[] AllAssets { protected set; get; }
		public IAssetInstance AssetInstance { protected set; get; }
		public EAssetStates States { protected set; get; }
		public int RefCount { private set; get; }
		public AssetOperationHandle Handle { private set; get; }
		public System.Action<AssetOperationHandle> Callback { set; get; }
		public bool IsDestroyed { private set; get; } = false;
		public bool IsDone
		{
			get
			{
				return States == EAssetStates.Success || States == EAssetStates.Fail;
			}
		}
		public bool IsValid
		{
			get
			{
				return IsDestroyed == false;
			}
		}
		public virtual float Progress
		{
			get
			{
				return 0;
			}
		}
		

		public AssetProviderBase(string assetPath, System.Type assetType)
		{
			AssetPath = assetPath;
			AssetName = System.IO.Path.GetFileName(assetPath);
			AssetType = assetType;
			States = EAssetStates.None;
			Handle = new AssetOperationHandle(this);
		}

		public abstract void Update();
		public virtual void Destory()
		{
			IsDestroyed = true;
		}

		public void Reference()
		{
			RefCount++;
		}
		public void Release()
		{
			if (RefCount <= 0)
				YooLogger.Warning("Asset provider reference count is already zero. There may be resource leaks !");

			RefCount--;
		}
		public bool CanDestroy()
		{
			if (IsDone == false)
				return false;

			return RefCount <= 0;
		}

		/// <summary>
		/// 是否为场景提供者
		/// </summary>
		public bool IsSceneProvider()
		{
			if (this is BundledSceneProvider || this is DatabaseSceneProvider)
				return true;
			else
				return false;
		}

		/// <summary>
		/// 等待异步执行完毕
		/// </summary>
		public virtual void WaitForAsyncComplete()
		{
			IsWaitForAsyncComplete = true;

			// 注意：主动轮询更新完成同步加载
			Update();

			// 验证结果
			if (IsDone == false)
			{
				YooLogger.Warning($"WaitForAsyncComplete failed to loading : {AssetPath}");
			}
		}

		/// <summary>
		/// 异步操作任务
		/// </summary>
		System.Threading.Tasks.Task<object> IAssetProvider.Task
		{
			get
			{
				var handle = WaitHandle;
				return System.Threading.Tasks.Task.Factory.StartNew(o =>
				{
					handle.WaitOne();
					return AssetObject as object;
				}, this);
			}
		}

		// 异步操作相关
		private System.Threading.EventWaitHandle _waitHandle;
		private System.Threading.WaitHandle WaitHandle
		{
			get
			{
				if (_waitHandle == null)
					_waitHandle = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);
				_waitHandle.Reset();
				return _waitHandle;
			}
		}
		protected void InvokeCompletion()
		{
			Callback?.Invoke(Handle);
			_waitHandle?.Set();
		}
	}
}