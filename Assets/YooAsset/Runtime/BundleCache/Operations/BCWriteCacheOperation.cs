
namespace YooAsset
{
    /// <summary>
    /// 写入缓存操作基类
    /// </summary>
    internal abstract class BCWriteCacheOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 写入缓存完成操作
    /// </summary>
    internal sealed class BCWriteCacheCompleteOperation : BCWriteCacheOperation
    {
        private readonly string _error;

        /// <summary>
        /// 创建写入缓存完成操作实例
        /// </summary>
        public BCWriteCacheCompleteOperation()
        {
            _error = null;
        }

        /// <summary>
        /// 创建写入缓存完成操作实例
        /// </summary>
        /// <param name="error">错误信息</param>
        public BCWriteCacheCompleteOperation(string error)
        {
            _error = error;
        }
        protected override void InternalStart()
        {
            if (string.IsNullOrEmpty(_error))
            {
                SetResult();
            }
            else
            {
                SetError(_error);
            }
        }
        protected override void InternalUpdate()
        {
        }
    }
}
