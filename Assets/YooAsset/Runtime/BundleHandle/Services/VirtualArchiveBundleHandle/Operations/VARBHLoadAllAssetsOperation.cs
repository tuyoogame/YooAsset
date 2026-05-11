
namespace YooAsset
{
    /// <summary>
    /// 全部资源加载操作（虚拟归档资源包不支持）
    /// </summary>
    internal sealed class VARBHLoadAllAssetsOperation : BHLoadAllAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(VARBHLoadAllAssetsOperation)} does not support loading all assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}
