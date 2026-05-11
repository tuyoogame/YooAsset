
namespace YooAsset
{
    /// <summary>
    /// 全部资源加载操作（归档资源包不支持）
    /// </summary>
    internal sealed class ARBHLoadAllAssetsOperation : BHLoadAllAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(ARBHLoadAllAssetsOperation)} does not support loading all assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}
