using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal interface IBundleQuery
    {
        /// <summary>
        /// 获取主资源包信息
        /// </summary>
        BundleInfo GetMainBundleInfo(AssetInfo assetInfo);

        /// <summary>
        /// 获取依赖的资源包信息集合
        /// </summary>
        List<BundleInfo> GetDependBundleInfos(AssetInfo assetPath);

        /// <summary>
        /// 获取主资源包名称
        /// </summary>
        string GetMainBundleName(int bundleID);
    }
}