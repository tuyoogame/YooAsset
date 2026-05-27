
namespace YooAsset
{
    internal sealed class DefaultBundleUnpackPolicy : IBundleUnpackPolicy
    {
        public bool IsUnpackBundle(BundleUnpackInfo unpackInfo)
        {
#if UNITY_ANDROID || UNITY_OPENHARMONY
            if (unpackInfo.IsEncrypted)
                return true;
            if (unpackInfo.BundleType == (int)EBundleType.RawBundle)
                return true;
            if (unpackInfo.BundleType == (int)EBundleType.ArchiveBundle)
                return true;
            return false;
#else
            return false;
#endif
        }
    }
}
