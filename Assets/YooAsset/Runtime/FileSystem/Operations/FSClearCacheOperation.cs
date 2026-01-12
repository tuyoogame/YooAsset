
namespace YooAsset
{
    /// <summary>
    /// 清理缓存操作的抽象基类
    /// </summary>
    internal abstract class FSClearCacheOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 清理缓存操作的立即完成实现
    /// </summary>
    internal sealed class FSClearCacheCompleteOperation : FSClearCacheOperation
    {
        private readonly string _error;

        internal FSClearCacheCompleteOperation()
        {
            _error = null;
        }
        internal FSClearCacheCompleteOperation(string error)
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