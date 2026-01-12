using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 描述资源包的运行时诊断信息
    /// </summary>
    [Serializable]
    internal class DiagnosticBundleInfo : IComparable<DiagnosticBundleInfo>
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        public string BundleName;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int ReferenceCount;

        /// <summary>
        /// 资源包的加载状态
        /// </summary>
        public string Status;

        /// <summary>
        /// 引用该资源包的其他资源包列表
        /// </summary>
        public List<string> Referencers;

        /// <inheritdoc />
        public int CompareTo(DiagnosticBundleInfo other)
        {
            return string.CompareOrdinal(BundleName, other.BundleName);
        }
    }
}