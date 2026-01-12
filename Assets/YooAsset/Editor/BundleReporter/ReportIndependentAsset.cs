using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 不被其他资源依赖的独立资源信息
    /// </summary>
    [Serializable]
    public class ReportIndependentAsset
    {
        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath;

        /// <summary>
        /// 资源GUID
        /// </summary>
        public string AssetGuid;

        /// <summary>
        /// 资源类型
        /// </summary>
        public string AssetType;

        /// <summary>
        /// 资源文件大小
        /// </summary>
        public long FileSize;

        public override string ToString()
        {
            return $"ReportIndependentAsset: {AssetPath} ({AssetType})";
        }
    }
}
