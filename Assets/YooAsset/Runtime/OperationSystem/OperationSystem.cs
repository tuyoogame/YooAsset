using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace YooAsset
{
	internal class OperationSystem
	{
		private static readonly List<AsyncOperationBase> _operations = new List<AsyncOperationBase>(100);
		private static readonly List<AsyncOperationBase> _newList = new List<AsyncOperationBase>(100);

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

			// 添加新的异步操作
			if (_newList.Count > 0)
			{
				_operations.AddRange(_newList);
				_newList.Clear();
			}

			// 更新所有的异步操作
			for (int i = _operations.Count - 1; i >= 0; i--)
			{
				if (IsBusy)
					break;

				var operation = _operations[i];
				operation.Update();
				if (operation.IsDone)
				{
					_operations.RemoveAt(i);
					operation.SetFinish(); //注意：如果业务端发生异常，保证异步操作提前移除。
				}
			}
		}

		/// <summary>
		/// 销毁异步操作系统
		/// </summary>
		public static void DestroyAll()
		{
			_operations.Clear();
			_newList.Clear();
			_watch = null;
			_frameTime = 0;
			MaxTimeSlice = long.MaxValue;
		}

		/// <summary>
		/// 开始处理异步操作类
		/// </summary>
		public static void StartOperation(AsyncOperationBase operation)
		{
			_newList.Add(operation);
			operation.SetStart();
			operation.Start();
		}
	}
}