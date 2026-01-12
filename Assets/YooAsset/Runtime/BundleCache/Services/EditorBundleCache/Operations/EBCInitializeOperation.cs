
namespace YooAsset
{
    /// <summary>
    /// 编辑器文件缓存初始化操作
    /// </summary>
    internal sealed class EBCInitializeOperation : BCInitializeOperation
    {
        private readonly EditorBundleCache _fileCache;

        public EBCInitializeOperation(EditorBundleCache cache)
        {
            _fileCache = cache;
        }
        protected override void InternalStart()
        {
            SetResult();
        }
        protected override void InternalUpdate()
        {
        }
    }
}
