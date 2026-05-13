
namespace YooAsset
{
    /// <summary>
    /// 资源加载方法枚举
    /// </summary>
    internal enum ELoadMethod
    {
        /// <summary>
        /// 无加载方法
        /// </summary>
        None = 0,

        /// <summary>
        /// 加载单个资源
        /// </summary> 
        LoadAsset,

        /// <summary>
        /// 加载子资源集合
        /// </summary>
        LoadSubAssets,

        /// <summary>
        /// 加载所有资源
        /// </summary>
        LoadAllAssets,

        /// <summary>
        /// 加载场景
        /// </summary>
        LoadScene,

        /// <summary>
        /// 加载资源包文件
        /// </summary>
        LoadBundleFile,
    }

    /// <summary>
    /// 资源信息类
    /// </summary>
    public class AssetInfo
    {
        private readonly PackageAsset _packageAsset;
        private string _assetKey;

        /// <summary>
        /// 所属包裹
        /// </summary>
        public string PackageName { get; private set; }

        /// <summary>
        /// 资源类型
        /// </summary>
        public System.Type AssetType { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// 加载方法
        /// </summary>
        internal ELoadMethod LoadMethod { get; set; }

        /// <summary>
        /// 资源描述
        /// </summary>
        internal PackageAsset Asset
        {
            get { return _packageAsset; }
        }

        internal string AssetKey
        {
            get
            {
                if (string.IsNullOrEmpty(_assetKey) == false)
                    return _assetKey;

                if (AssetType == null)
                    _assetKey = $"[{AssetPath}][null]";
                else
                    _assetKey = $"[{AssetPath}][{AssetType.Name}]";
                return _assetKey;
            }
        }

        /// <summary>
        /// 资源信息是否有效
        /// </summary>
        public bool IsValid
        {
            get
            {
                return _packageAsset != null;
            }
        }

        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address
        {
            get
            {
                if (_packageAsset == null)
                    return string.Empty;
                return _packageAsset.Address;
            }
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath
        {
            get
            {
                if (_packageAsset == null)
                    return string.Empty;
                return _packageAsset.AssetPath;
            }
        }

        /// <summary>
        /// 创建有效的资源信息实例
        /// </summary>
        /// <param name="packageName">所属包裹名称</param>
        /// <param name="packageAsset">清单中的资源描述</param>
        /// <param name="assetType">资源类型</param>
        internal AssetInfo(string packageName, PackageAsset packageAsset, System.Type assetType)
        {
            if (packageAsset == null)
                throw new YooInternalException("Package asset cannot be null.");

            _assetKey = string.Empty;
            _packageAsset = packageAsset;
            PackageName = packageName;
            AssetType = assetType;
            Error = string.Empty;
        }

        /// <summary>
        /// 创建无效的资源信息实例
        /// </summary>
        /// <param name="packageName">所属包裹名称</param>
        /// <param name="error">错误信息</param>
        internal AssetInfo(string packageName, string error)
        {
            _assetKey = string.Empty;
            _packageAsset = null;
            PackageName = packageName;
            AssetType = null;
            Error = error;
        }
    }
}