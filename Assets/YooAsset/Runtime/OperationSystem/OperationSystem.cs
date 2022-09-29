using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
	internal class OperationSystem
	{
		private static readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(100);

		// 计时器相关
		private static Stopwatch _watch;
		private static long _frameTime;

		/// <summary>
		/// 异步操作的最小时间片段
		/// </summary>
		public static long MaxTimeSlice { set; get; } = long.MaxValue;

		/// <summary>
		/// 处理器是否繁忙
		/// </summary>
		public static bool IsBusy
		{
			get
			{
				return _watch.ElapsedMilliseconds - _frameTime >= MaxTimeSlice;
			}
		}


		/// <summary>
		/// 初始化异步操作系统
		/// </summary>
		public static void Initialize()
		{
			_watch = Stopwatch.StartNew();
		}

		/// <summary>
		/// 更新异步操作系统
		/// </summary>
		public static void Update()
		{
			_frameTime = _watch.ElapsedMilliseconds;

			for (int i = _operations.Count - 1; i >= 0; i--)
			{
				if (IsBusy)
					return;

				var operation = _operations[i];
				operation.Update();
				if (operation.IsDone)
				{
					_operations.RemoveAt(i);
					operation.Finish();
				}
			}
		}

		/// <summary>
		/// 销毁异步操作系统
		/// </summary>
		public static void DestroyAll()
		{
			_operations.Clear();
			_watch = null;
			_frameTime = 0;
			MaxTimeSlice = long.MaxValue;
		}

		/// <summary>
		/// 开始处理异步操作类
		/// </summary>
		public static void StartOperation(AsyncOperationBase operationBase)
		{
			_operations.Add(operationBase);
			operationBase.Start();
		}
	}
}