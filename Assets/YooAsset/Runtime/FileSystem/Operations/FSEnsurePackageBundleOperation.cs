
namespace YooAsset
{
    /// <summary>
    /// 确保资源包已就绪的操作基类
    /// </summary>
    internal abstract class FSEnsurePackageBundleOperation : AsyncOperationBase
    {
        /// <summary>
        /// 资源包文件的本地路径
        /// </summary>
        public string BundleFilePath { get; protected set; }
    }

    /// <summary>
    /// 确保资源包已就绪的失败实现
    /// </summary>
    internal sealed class FSEnsurePackageBundleFailureOperation : FSEnsurePackageBundleOperation
    {
        private readonly string _error;

        internal FSEnsurePackageBundleFailureOperation(string error)
        {
            _error = error;
        }

        protected override void InternalStart()
        {
            SetError(_error);
        }
        protected override void InternalUpdate()
        {
        }
    }
}
