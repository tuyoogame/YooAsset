
namespace YooAsset
{
    /// <summary>
    /// 确保资源包文件已就绪的操作选项
    /// </summary>
    public readonly struct EnsureBundleFileOptions
    {
        /// <summary>
        /// 资源定位地址
        /// </summary>
        public string Location { get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo AssetInfo { get; }

        /// <summary>
        /// 创建确保资源包文件已就绪的选项实例
        /// </summary>
        /// <param name="location">资源定位地址</param>
        public EnsureBundleFileOptions(string location)
        {
            Location = location;
            AssetInfo = null;
        }

        /// <summary>
        /// 创建确保资源包文件已就绪的选项实例
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public EnsureBundleFileOptions(AssetInfo assetInfo)
        {
            Location = null;
            AssetInfo = assetInfo;
        }
    }
}
