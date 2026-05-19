#if YOOASSET_LEGACY_API
using System;

namespace YooAsset
{
    [Obsolete("Use EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualAssetBundle) instead.")]
    public static class EditorSimulateModeHelper
    {
        public static PackageBuildResult SimulateBuild(string packageName)
        {
            return EditorSimulateBuildInvoker.Build(packageName, (int)EBundleType.VirtualAssetBundle);
        }
    }
}
#endif
