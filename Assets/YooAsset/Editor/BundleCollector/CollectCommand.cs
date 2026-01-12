
namespace YooAsset.Editor
{
    /// <summary>
    /// 资源收集标记位
    /// </summary>
    [System.Flags]
    public enum ECollectFlags
    {
        /// <summary>
        /// 无标记
        /// </summary>
        None = 0,

        /// <summary>
        /// 不收集依赖资源
        /// </summary>
        IgnoreGetDependencies = 1 << 0,

        /// <summary>
        /// 忽略静态收集器
        /// </summary>
        IgnoreStaticCollector = 1 << 1,

        /// <summary>
        /// 忽略依赖收集器
        /// </summary>
        IgnoreDependCollector = 1 << 2,
    }

    /// <summary>
    /// 资源收集命令
    /// </summary>
    public class CollectCommand
    {
        private AssetDependencyCache _assetDependency;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 忽略规则实例
        /// </summary>
        public IAssetIgnoreRule IgnoreRule { private set; get; }


        /// <summary>
        /// 设置模拟构建模式
        /// </summary>
        /// <param name="value">是否为模拟构建</param>
        public void SetSimulateBuild(bool value)
        {
            SetFlag(ECollectFlags.IgnoreGetDependencies, value);
            SetFlag(ECollectFlags.IgnoreStaticCollector, value);
            SetFlag(ECollectFlags.IgnoreDependCollector, value);
        }

        /// <summary>
        /// 窗口收集模式
        /// </summary>
        public ECollectFlags CollectFlags { set; get; } = ECollectFlags.None;

        /// <summary>
        /// 是否启用资源包名唯一化
        /// </summary>
        public bool UniqueBundleName { set; get; }

        /// <summary>
        /// 是否使用资源依赖数据库
        /// </summary>
        public bool UseAssetDependencyDB { set; get; }

        /// <summary>
        /// 是否启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable { set; get; }

        /// <summary>
        /// 是否支持无后缀名的资源定位地址
        /// </summary>
        public bool SupportExtensionless { set; get; }

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower { set; get; }

        /// <summary>
        /// 是否包含资源GUID数据
        /// </summary>
        public bool IncludeAssetGUID { set; get; }

        /// <summary>
        /// 是否自动收集所有着色器
        /// </summary>
        public bool AutoCollectShaders { set; get; }

        /// <summary>
        /// 资源依赖缓存
        /// </summary>
        public AssetDependencyCache AssetDependency
        {
            get
            {
                if (_assetDependency == null)
                    _assetDependency = new AssetDependencyCache(UseAssetDependencyDB);
                return _assetDependency;
            }
        }

        /// <summary>
        /// 创建 CollectCommand 实例
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="ignoreRule">忽略规则实例</param>
        public CollectCommand(string packageName, IAssetIgnoreRule ignoreRule)
        {
            PackageName = packageName;
            IgnoreRule = ignoreRule;
        }

        /// <summary>
        /// 设置标记位
        /// </summary>
        /// <param name="flag">标记位</param>
        /// <param name="isOn">是否开启</param>
        public void SetFlag(ECollectFlags flag, bool isOn)
        {
            if (isOn)
                CollectFlags |= flag;
            else
                CollectFlags &= ~flag;
        }

        /// <summary>
        /// 查询标记位
        /// </summary>
        /// <param name="flag">标记位</param>
        /// <returns>如果标记位已开启返回 true</returns>
        public bool IsFlagSet(ECollectFlags flag)
        {
            return (CollectFlags & flag) != 0;
        }
    }
}
