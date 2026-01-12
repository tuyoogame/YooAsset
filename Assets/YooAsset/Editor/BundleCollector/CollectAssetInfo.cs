using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 收集的资源信息
    /// </summary>
    public class CollectAssetInfo
    {
        /// <summary>
        /// 收集器类型
        /// </summary>
        public ECollectorType CollectorType { private set; get; }

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName { private set; get; }

        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address { private set; get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public EditorAssetInfo AssetInfo { private set; get; }

        /// <summary>
        /// 资源分类标签
        /// </summary>
        public List<string> AssetTags { private set; get; }

        /// <summary>
        /// 依赖的资源列表
        /// </summary>
        public List<EditorAssetInfo> DependAssets { set; get; } = new List<EditorAssetInfo>();


        /// <summary>
        /// 创建 CollectAssetInfo 实例
        /// </summary>
        /// <param name="collectorType">收集器类型</param>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="address">可寻址地址</param>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="assetTags">资源分类标签</param>
        public CollectAssetInfo(ECollectorType collectorType, string bundleName, string address, EditorAssetInfo assetInfo, List<string> assetTags)
        {
            CollectorType = collectorType;
            BundleName = bundleName;
            Address = address;
            AssetInfo = assetInfo;
            AssetTags = assetTags;
        }
    }
}