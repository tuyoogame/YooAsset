using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	public abstract class AsyncOperationBase : IEnumerator
	{
		// 用户请求的回调
		private Action<AsyncOperationBase> _callback;

		/// <summary>
		/// 状态
		/// </summary>
		public EOperationStatus Status { get; protected set; } = EOperationStatus.None;

		/// <summary>
		/// 错误信息
		/// </summary>
		public string Error { get; protected set; } = string.Empty;

		/// <summary>
		/// 是否已经完成
		/// </summary>
		public bool IsDone
		{
			get
			{
				return Status == EOperationStatus.Failed || Status == EOperationStatus.Succeed;
			}
		}

		/// <summary>
		/// 完成事件
		/// </summary>
		public event Action<AsyncOperationBase> Completed
		{
			add
			{
				if (IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				_callback -= value;
			}
		}

		internal abstract void Start();
		internal abstract void Update();
		internal void Finish()
		{
			_callback?.Invoke(this);
			_waitHandle?.Set();
		}

		#region 异步编程相关
		public bool MoveNext()
		{
			return !IsDone;
		}
		public void Reset()
		{
		}
		public object Current => null;

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
		public System.Threading.Tasks.Task Task
		{
			get
			{
				var handle = WaitHandle;
				return System.Threading.Tasks.Task.Factory.StartNew(o =>
				{
					handle.WaitOne();
				}, this);
			}
		}
		#endregion
	}
}