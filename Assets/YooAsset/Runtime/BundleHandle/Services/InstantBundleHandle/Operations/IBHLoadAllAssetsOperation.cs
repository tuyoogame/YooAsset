#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
namespace YooAsset
{
    /// <summary>
    /// 全部资源加载操作（InstantBundle不支持）
    /// </summary>
    internal sealed class IBHLoadAllAssetsOperation : BHLoadAllAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(IBHLoadAllAssetsOperation)} does not support loading all assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}
#endif
