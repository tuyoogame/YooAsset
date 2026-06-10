
namespace YooAsset
{
    /// <summary>
    /// 资源包裹的详细信息
    /// </summary>
    public class PackageDetails
    {
        private readonly PackageManifest _manifest;

        /// <summary>
        /// 文件版本
        /// </summary>
        public int FileVersion
        {
            get { return _manifest.FileVersion; }
        }

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable
        {
            get { return _manifest.EnableAddressable; }
        }

        /// <summary>
        /// 支持无后缀名的资源定位地址
        /// </summary>
        public bool SupportExtensionless
        {
            get { return _manifest.SupportExtensionless; }
        }

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower
        {
            get { return _manifest.LocationToLower; }
        }

        /// <summary>
        /// 包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGuid
        {
            get { return _manifest.IncludeAssetGuid; }
        }

        /// <summary>
        /// 使用可寻址地址代替资源路径
        /// </summary>
        public bool ReplaceAssetPathWithAddress
        {
            get { return _manifest.ReplaceAssetPathWithAddress; }
        }

        /// <summary>
        /// 文件名称样式
        /// </summary>
        public int OutputNameStyle
        {
            get { return _manifest.OutputNameStyle; }
        }

        /// <summary>
        /// 构建资源包类型
        /// </summary>
        public int BuildBundleType
        {
            get { return _manifest.BuildBundleType; }
        }

        /// <summary>
        /// 构建管线名称
        /// </summary>
        public string BuildPipeline
        {
            get { return _manifest.BuildPipeline; }
        }

        /// <summary>
        /// 资源包裹名称
        /// </summary>
        public string PackageName
        {
            get { return _manifest.PackageName; }
        }

        /// <summary>
        /// 资源包裹的版本信息
        /// </summary>
        public string PackageVersion
        {
            get { return _manifest.PackageVersion; }
        }

        /// <summary>
        /// 资源包裹的备注信息
        /// </summary>
        public string PackageNote
        {
            get { return _manifest.PackageNote; }
        }

        /// <summary>
        /// 主资源文件总数
        /// </summary>
        public int AssetTotalCount
        {
            get { return _manifest.AssetList.Count; }
        }

        /// <summary>
        /// 资源包文件总数
        /// </summary>
        public int BundleTotalCount
        {
            get { return _manifest.BundleList.Count; }
        }

        internal PackageDetails(PackageManifest manifest)
        {
            _manifest = manifest;
        }

        /// <summary>
        /// 返回包裹详细信息的字符串描述
        /// </summary>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"FileVersion : {FileVersion}");
            sb.AppendLine($"PackageName : {PackageName}");
            sb.AppendLine($"PackageVersion : {PackageVersion}");
            sb.AppendLine($"PackageNote : {PackageNote}");
            sb.AppendLine($"BuildPipeline : {BuildPipeline}");
            sb.AppendLine($"BuildBundleType : {BuildBundleType}");
            sb.AppendLine($"OutputNameStyle : {OutputNameStyle}");
            sb.AppendLine($"EnableAddressable : {EnableAddressable}");
            sb.AppendLine($"SupportExtensionless : {SupportExtensionless}");
            sb.AppendLine($"LocationToLower : {LocationToLower}");
            sb.AppendLine($"IncludeAssetGuid : {IncludeAssetGuid}");
            sb.AppendLine($"ReplaceAssetPathWithAddress : {ReplaceAssetPathWithAddress}");
            sb.AppendLine($"AssetTotalCount : {AssetTotalCount}");
            sb.AppendLine($"BundleTotalCount : {BundleTotalCount}");
            return sb.ToString();
        }
    }
}
