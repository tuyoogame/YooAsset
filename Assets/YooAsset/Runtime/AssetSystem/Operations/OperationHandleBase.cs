using System.Collections;

namespace YooAsset
{
	public abstract class OperationHandleBase : IEnumerator
	{
		internal AssetProviderBase _provider { private set; get; }
		
		internal OperationHandleBase(AssetProviderBase provider)
		{
			_provider = provider;
		}
		internal abstract void InvokeCallback();

		/// <summary>
		/// 当前状态
		/// </summary>
		public EOperationStatus Status
		{
			get
			{
				if (IsValid == false)
					return EOperationStatus.None;
				if (_provider.Status == AssetProviderBase.EStatus.Fail)
					return EOperationStatus.Failed;
				else if (_provider.Status == AssetProviderBase.EStatus.Success)
					return EOperationStatus.Succeed;
				else
					return EOperationStatus.None;
			}
		}

		/// <summary>
		/// 加载进度
		/// </summary>
		public float Progress
		{
			get
			{
				if (IsValid == false)
					return 0;
				return _provider.Progress;
			}
		}

		/// <summary>
		/// 是否加载完毕
		/// </summary>
		public bool IsDone
		{
			get
			{
				if (IsValid == false)
					return false;
				return _provider.IsDone;
			}
		}

		/// <summary>
		/// 句柄是否有效
		/// </summary>
		public bool IsValid
		{
			get
			{
				return _provider != null && _provider.IsDestroyed == false;
			}
		}

		/// <summary>
		/// 释放句柄
		/// </summary>
		internal void ReleaseInternal()
		{
			if (IsValid == false)
				return;
			_provider.ReleaseHandle(this);
			_provider = null;
		}

		#region 异步操作相关
		/// <summary>
		/// 异步操作任务
		/// </summary>
		public System.Threading.Tasks.Task<object> Task
		{
			get { return _provider.Task; }
		}

		// 协程相关
		bool IEnumerator.MoveNext()
		{
			return !IsDone;
		}
		void IEnumerator.Reset()
		{
		}
		object IEnumerator.Current
		{
			get { return _provider; }
		}
		#endregion
	}
}