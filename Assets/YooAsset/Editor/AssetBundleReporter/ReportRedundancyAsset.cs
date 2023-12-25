using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    [Serializable]
    public class ReportRedundancyAsset
    {
        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo AssetInfo;

        /// <summary>
        /// 资源文件大小
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 冗余的资源包数量
        /// </summary>
        public int Number;
    }
}