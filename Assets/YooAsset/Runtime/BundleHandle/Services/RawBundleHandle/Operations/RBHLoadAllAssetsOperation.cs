
namespace YooAsset
{
    /// <summary>
    /// 全部资源加载操作（原生资源包不支持）
    /// </summary>
    internal sealed class RBHLoadAllAssetsOperation : BHLoadAllAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(RBHLoadAllAssetsOperation)} does not support loading all assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}