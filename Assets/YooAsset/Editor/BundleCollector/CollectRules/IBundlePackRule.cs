
namespace YooAsset.Editor
{
    /// <summary>
    /// 打包规则的输入数据
    /// </summary>
    public readonly struct BundlePackRuleData
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get; }

        /// <summary>
        /// 收集路径
        /// </summary>
        public string CollectPath { get; }

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public string UserData { get; }

        /// <summary>
        /// 创建 BundlePackRuleData 实例
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <param name="collectPath">收集路径</param>
        /// <param name="groupName">分组名称</param>
        /// <param name="userData">用户自定义数据</param>
        public BundlePackRuleData(string assetPath, string collectPath, string groupName, string userData)
        {
            AssetPath = assetPath;
            CollectPath = collectPath;
            GroupName = groupName;
            UserData = userData;
        }
    }

    /// <summary>
    /// 打包规则的输出结果
    /// </summary>
    public readonly struct BundlePackRuleResult
    {
        private readonly string _bundleName;
        private readonly string _bundleExtension;

        /// <summary>
        /// 创建 BundlePackRuleResult 实例
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="bundleExtension">资源包文件扩展名</param>
        public BundlePackRuleResult(string bundleName, string bundleExtension)
        {
            _bundleName = bundleName;
            _bundleExtension = bundleExtension;
        }

        /// <summary>
        /// 检查是否为有效结果
        /// </summary>
        /// <returns>名称和扩展名均不为空时返回 true</returns>
        public bool IsValid()
        {
            return string.IsNullOrEmpty(_bundleName) == false && string.IsNullOrEmpty(_bundleExtension) == false;
        }

        /// <summary>
        /// 获取资源包全名称
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="uniqueBundleName">是否启用资源包名唯一化</param>
        /// <returns>格式化后的资源包全名称</returns>
        public string GetBundleName(string packageName, bool uniqueBundleName)
        {
            string fullName;
            string bundleName = EditorPathUtility.GetRegularPath(_bundleName).Replace('/', '_').Replace('.', '_').Replace(" ", "_").ToLower();
            if (uniqueBundleName)
                fullName = $"{packageName}_{bundleName}.{_bundleExtension}";
            else
                fullName = $"{bundleName}.{_bundleExtension}";
            return fullName.ToLower();
        }

        /// <summary>
        /// 获取共享资源包全名称
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="uniqueBundleName">是否启用资源包名唯一化</param>
        /// <returns>格式化后的共享资源包全名称</returns>
        public string GetShareBundleName(string packageName, bool uniqueBundleName)
        {
            string fullName;
            string bundleName = EditorPathUtility.GetRegularPath(_bundleName).Replace('/', '_').Replace('.', '_').Replace(" ", "_").ToLower();
            if (uniqueBundleName)
                fullName = $"{packageName}_share_{bundleName}.{_bundleExtension}";
            else
                fullName = $"share_{bundleName}.{_bundleExtension}";
            return fullName.ToLower();
        }
    }

    /// <summary>
    /// 资源打包规则接口
    /// </summary>
    public interface IBundlePackRule
    {
        /// <summary>
        /// 获取打包规则结果
        /// </summary>
        /// <param name="data">打包规则数据</param>
        /// <returns>打包规则结果</returns>
        BundlePackRuleResult GetPackRuleResult(BundlePackRuleData data);
    }
}
