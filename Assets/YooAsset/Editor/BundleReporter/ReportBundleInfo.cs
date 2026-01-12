using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建报告中单个资源包的详细信息
    /// </summary>
    [Serializable]
    public class ReportBundleInfo
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName;

        /// <summary>
        /// 文件哈希值
        /// </summary>
        public string FileHash;

        /// <summary>
        /// 文件校验码
        /// </summary>
        public uint FileCrc;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 是否为加密文件
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// 资源包标签集合
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 依赖的资源包集合
        /// </summary>
        /// <remarks>引擎层构建查询结果</remarks>
        public List<string> DependBundles = new List<string>();

        /// <summary>
        /// 引用该资源包的资源包集合
        /// </summary>
        /// <remarks>记录哪些资源包依赖了当前资源包</remarks>
        public List<string> ReferenceBundles = new List<string>();

        /// <summary>
        /// 资源包内部所有资源
        /// </summary>
        public List<EditorAssetInfo> BundleContents = new List<EditorAssetInfo>();

        /// <summary>
        /// 获取资源分类标签的字符串
        /// </summary>
        /// <returns>以分号分隔的标签字符串，无标签时返回空字符串。</returns>
        public string GetTagsString()
        {
            if (Tags != null)
                return String.Join(";", Tags);
            else
                return string.Empty;
        }
    }
}