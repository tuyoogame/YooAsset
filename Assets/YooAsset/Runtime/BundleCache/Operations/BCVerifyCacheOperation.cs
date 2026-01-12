
namespace YooAsset
{
    /// <summary>
    /// 验证缓存操作基类
    /// </summary>
    internal abstract class BCVerifyCacheOperation : AsyncOperationBase
    {
    }

    /// <summary>
    /// 验证缓存完成操作
    /// </summary>
    internal sealed class BCVerifyCacheCompleteOperation : BCVerifyCacheOperation
    {
        private readonly string _error;
        

        /// <summary>
        /// 创建验证缓存完成操作实例
        /// </summary>
        public BCVerifyCacheCompleteOperation()
        {
            _error = null;
        }

        /// <summary>
        /// 创建验证缓存完成操作实例
        /// </summary>
        /// <param name="error">错误信息</param>
        public BCVerifyCacheCompleteOperation(string error)
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
