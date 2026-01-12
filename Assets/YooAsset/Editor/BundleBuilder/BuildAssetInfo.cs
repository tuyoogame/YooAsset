using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建资源信息，记录单个资源在构建过程中的元数据
    /// </summary>
    public class BuildAssetInfo
    {
        private bool _isAddAssetTags = false;
        private readonly HashSet<string> _referenceBundleNames = new HashSet<string>();
        private readonly List<string> _assetTags = new List<string>();

        /// <summary>
        /// 收集器类型
        /// </summary>
        public ECollectorType CollectorType { private set; get; }

        /// <summary>
        /// 资源包完整名称
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
        /// 资源的分类标签
        /// </summary>
        public IReadOnlyList<string> AssetTags
        {
            get { return _assetTags; }
        }

        /// <summary>
        /// 依赖的所有资源
        /// 注意：包括零依赖资源和冗余资源（资源包名无效）
        /// </summary>
        public List<BuildAssetInfo> AllDependAssetInfos { private set; get; }


        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="collectorType">收集器类型</param>
        /// <param name="bundleName">所属的资源包名称</param>
        /// <param name="address">可寻址地址</param>
        /// <param name="assetInfo">资源信息</param>
        public BuildAssetInfo(ECollectorType collectorType, string bundleName, string address, EditorAssetInfo assetInfo)
        {
            CollectorType = collectorType;
            BundleName = bundleName;
            Address = address;
            AssetInfo = assetInfo;
        }

        /// <summary>
        /// 创建零依赖或冗余的实例
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public BuildAssetInfo(EditorAssetInfo assetInfo)
        {
            CollectorType = ECollectorType.None;
            BundleName = string.Empty;
            Address = string.Empty;
            AssetInfo = assetInfo;
        }

        /// <summary>
        /// 设置所有依赖的资源
        /// </summary>
        /// <param name="dependAssetInfos">依赖资源信息列表</param>
        public void SetDependAssetInfos(List<BuildAssetInfo> dependAssetInfos)
        {
            if (AllDependAssetInfos != null)
                throw new YooInternalException("Should never get here.");

            AllDependAssetInfos = dependAssetInfos;
        }

        /// <summary>
        /// 设置资源包名称
        /// </summary>
        /// <param name="bundleName">资源包完整名称</param>
        public void SetBundleName(string bundleName)
        {
            if (HasBundleName())
                throw new YooInternalException("Should never get here.");

            BundleName = bundleName;
        }

        /// <summary>
        /// 添加资源的分类标签
        /// </summary>
        /// <remarks>仅在首次调用时生效，不可重复设置。</remarks>
        /// <param name="tags">资源分类标签列表</param>
        public void AddAssetTags(List<string> tags)
        {
            if (_isAddAssetTags)
                throw new YooInternalException("Should never get here.");
            _isAddAssetTags = true;

            foreach (var tag in tags)
            {
                if (_assetTags.Contains(tag) == false)
                {
                    _assetTags.Add(tag);
                }
            }
        }

        /// <summary>
        /// 添加关联的资源包名称
        /// </summary>
        /// <param name="bundleName">引用该资源的资源包名称</param>
        public void AddReferenceBundleName(string bundleName)
        {
            if (string.IsNullOrEmpty(bundleName))
                throw new YooInternalException("Should never get here.");

            if (_referenceBundleNames.Contains(bundleName) == false)
                _referenceBundleNames.Add(bundleName);
        }

        /// <summary>
        /// 检查是否已分配资源包名称
        /// </summary>
        /// <returns>已分配返回 true，否则返回 false</returns>
        public bool HasBundleName()
        {
            if (string.IsNullOrEmpty(BundleName))
                return false;
            else
                return true;
        }

        /// <summary>
        /// 获取关联资源包的数量
        /// </summary>
        /// <returns>引用该资源的资源包数量</returns>
        public int GetReferenceBundleCount()
        {
            return _referenceBundleNames.Count;
        }
    }
}