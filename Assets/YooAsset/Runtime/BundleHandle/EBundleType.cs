
namespace YooAsset
{
    /// <summary>
    /// 资源包的类型
    /// </summary>
    public enum EBundleType
    {
        /// <summary>
        /// 未指定类型
        /// </summary>
        None = 0,

        /// <summary>
        /// 虚拟资源包（模拟AssetBundle）
        /// </summary>
        VirtualBundle = 1,

        /// <summary>
        /// Unity引擎资源包
        /// </summary>
        AssetBundle = 2,

        /// <summary>
        /// 原生文件资源包
        /// </summary>
        RawBundle = 3,
        
        /// <summary>
        /// 团结引擎资源包
        /// </summary>
        InstantBundle = 4,
    }
}