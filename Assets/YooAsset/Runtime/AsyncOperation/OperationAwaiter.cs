using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace YooAsset
{
    /// <summary>
    /// 支持异步编程的自定义 Awaiter
    /// </summary>
    public readonly struct OperationAwaiter : ICriticalNotifyCompletion
    {
        private readonly AsyncOperationBase _operation;

        /// <summary>
        /// 创建操作等待器实例
        /// </summary>
        /// <param name="operation">要等待的异步操作</param>
        public OperationAwaiter(AsyncOperationBase operation)
        {
            _operation = operation;
        }

        /// <inheritdoc />
        public bool IsCompleted => _operation.IsDone;

        /// <summary>
        /// 获取操作结果
        /// </summary>
        /// <remarks>
        /// 业务失败不视为异常，此处不抛出异常。
        /// </remarks>
        public void GetResult()
        {
        }

        /// <inheritdoc />
        public void OnCompleted(Action continuation)
        {
            UnsafeOnCompleted(continuation);
        }

        /// <inheritdoc />
        public void UnsafeOnCompleted(Action continuation)
        {
            _operation.Completed += (op) => continuation();
        }
    }
}
