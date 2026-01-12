using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 内置资源目录
    /// </summary>
    [Serializable]
    internal class BuiltinCatalog
    {
        /// <summary>
        /// 内置资源目录条目
        /// </summary>
        [Serializable]
        public class CatalogEntry
        {
            /// <summary>
            /// 资源包唯一标识
            /// </summary>
            public string BundleGuid;

            /// <summary>
            /// 资源包文件名
            /// </summary>
            public string FileName;
        }

        /// <summary>
        /// 文件版本
        /// </summary>
        public int FileVersion;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion;

        /// <summary>
        /// 目录条目列表
        /// </summary>
        public List<CatalogEntry> Entries = new List<CatalogEntry>();
    }
}