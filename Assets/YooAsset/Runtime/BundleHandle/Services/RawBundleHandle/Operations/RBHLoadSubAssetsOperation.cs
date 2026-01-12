
namespace YooAsset
{
    /// <summary>
    /// 子资源加载操作（原生资源包不支持）
    /// </summary>
    internal sealed class RBHLoadSubAssetsOperation : BHLoadSubAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(RBHLoadSubAssetsOperation)} does not support loading sub-assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}