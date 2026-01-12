using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建报告中单个资源的详细信息
    /// </summary>
    [Serializable]
    public class ReportAssetInfo
    {
        /// <summary>
        /// 可寻址地址
        /// </summary>
        public string Address;

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 资源 GUID
        /// </summary>
        /// <remarks>Meta 文件记录的 GUID</remarks>
        public string AssetGuid;

        /// <summary>
        /// 资源的分类标签
        /// </summary>
        public string[] AssetTags;

        /// <summary>
        /// 所属资源包名称
        /// </summary>
        public string MainBundleName;

        /// <summary>
        /// 所属资源包的大小
        /// </summary>
        public long MainBundleSize;

        /// <summary>
        /// 依赖的资源集合
        /// </summary>
        public List<EditorAssetInfo> DependAssets = new List<EditorAssetInfo>();

        /// <summary>
        /// 依赖的资源包集合
        /// </summary>
        /// <remarks>框架层收集查询结果</remarks>
        public List<string> DependBundles = new List<string>();
    }
}
