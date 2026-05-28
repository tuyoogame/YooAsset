#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
namespace YooAsset
{
    /// <summary>
    /// 子资源加载操作（InstantBundle不支持）
    /// </summary>
    internal sealed class IBHLoadSubAssetsOperation : BHLoadSubAssetsOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(IBHLoadSubAssetsOperation)} does not support loading sub-assets.");
        }
        protected override void InternalUpdate()
        {
        }
    }
}
#endif
