using System;

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
        /// Unity引擎资源包
        /// </summary>
        AssetBundle = 2,

        /// <summary>
        /// 原生文件资源包
        /// </summary>
        RawBundle = 3,

        /// <summary>
        /// 归档文件资源包
        /// </summary>
        ArchiveBundle = 4,

        /// <summary>
        /// 团结引擎资源包
        /// </summary>
        InstantBundle = 5,


        /// <summary>
        /// 虚拟 Unity 引擎资源包（编辑器模拟 AssetBundle）
        /// </summary>
        VirtualAssetBundle = 12,

        /// <summary>
        /// 虚拟原生文件资源包（编辑器模拟 RawBundle）
        /// </summary>
        VirtualRawBundle = 13,

        /// <summary>
        /// 虚拟归档文件资源包（编辑器模拟 ArchiveBundle）
        /// </summary>
        VirtualArchiveBundle = 14,

#if YOOASSET_LEGACY_API
        [Obsolete("Use VirtualAssetBundle instead.")]
        VirtualBundle = VirtualAssetBundle,
#endif
    }
}