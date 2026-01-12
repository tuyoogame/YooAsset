
namespace YooAsset
{
    /// <summary>
    /// 资源包裹的详细信息，用于外部查询包裹配置。
    /// </summary>
    public class PackageDetails
    {
        /// <summary>
        /// 文件版本
        /// </summary>
        public int FileVersion { get; internal set; }

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable { get; internal set; }

        /// <summary>
        /// 支持无后缀名的资源定位地址
        /// </summary>
        public bool SupportExtensionless { get; internal set; }

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower { get; internal set; }

        /// <summary>
        /// 包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGuid { get; internal set; }

        /// <summary>
        /// 使用可寻址地址代替资源路径
        /// </summary>
        public bool ReplaceAssetPathWithAddress { get; internal set; }

        /// <summary>
        /// 文件名称样式
        /// </summary>
        public int OutputNameStyle { get; internal set; }

        /// <summary>
        /// 构建资源包类型
        /// </summary>
        public int BuildBundleType { get; internal set; }

        /// <summary>
        /// 构建管线名称
        /// </summary>
        public string BuildPipeline { get; internal set; }

        /// <summary>
        /// 资源包裹名称
        /// </summary>
        public string PackageName { get; internal set; }

        /// <summary>
        /// 资源包裹的版本信息
        /// </summary>
        public string PackageVersion { get; internal set; }

        /// <summary>
        /// 资源包裹的备注信息
        /// </summary>
        public string PackageNote { get; internal set; }

        /// <summary>
        /// 主资源文件总数
        /// </summary>
        public int AssetTotalCount { get; internal set; }

        /// <summary>
        /// 资源包文件总数
        /// </summary>
        public int BundleTotalCount { get; internal set; }
    }
}