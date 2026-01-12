
namespace YooAsset
{
    /// <summary>
    /// 清理缓存操作基类
    /// </summary>
    internal abstract class BCClearCacheOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 清理缓存完成操作
    /// </summary>
    internal sealed class BCClearCacheCompleteOperation : BCClearCacheOperation
    {
        private readonly string _error;

        /// <summary>
        /// 创建清理缓存完成操作实例
        /// </summary>
        public BCClearCacheCompleteOperation()
        {
            _error = null;
        }

        /// <summary>
        /// 创建清理缓存完成操作实例
        /// </summary>
        /// <param name="error">错误信息</param>
        public BCClearCacheCompleteOperation(string error)
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
