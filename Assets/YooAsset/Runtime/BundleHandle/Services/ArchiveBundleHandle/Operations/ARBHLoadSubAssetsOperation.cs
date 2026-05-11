
namespace YooAsset
{
    /// <summary>
    /// 子资源加载操作（归档资源包不支持）
    /// </summary>
    internal sealed class ARBHLoadSubAssetsOperation : BHLoadSubAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(ARBHLoadSubAssetsOperation)} does not support loading sub-assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}
