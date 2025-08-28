using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
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
        public uint FileCRC;

        /// <summary>
        /// 文件大小（字节数）
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 加密文件
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// 资源包标签集合
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// 依赖的资源包集合
        /// 说明：引擎层构建查询结果
        /// </summary>
        public List<string> DependBundles = new List<string>();

        /// <summary>
        /// 引用该资源包的资源包集合
        /// 说明：谁依赖该资源包
        /// </summary>
        public List<string> ReferenceBundles = new List<string>();

        /// <summary>
        /// 资源包内部所有资产
        /// </summary>
        public List<AssetInfo> BundleContents = new List<AssetInfo>();

        /// <summary>
        /// 获取资源分类标签的字符串
        /// </summary>
        public string GetTagsString()
        {
            if (Tags != null)
                return String.Join(";", Tags);
            else
                return string.Empty;
        }
    }
}