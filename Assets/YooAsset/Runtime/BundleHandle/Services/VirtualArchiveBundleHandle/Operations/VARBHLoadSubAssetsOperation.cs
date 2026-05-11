
namespace YooAsset
{
    /// <summary>
    /// 子资源加载操作（虚拟归档资源包不支持）
    /// </summary>
    internal sealed class VARBHLoadSubAssetsOperation : BHLoadSubAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(VARBHLoadSubAssetsOperation)} does not support loading sub-assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}
