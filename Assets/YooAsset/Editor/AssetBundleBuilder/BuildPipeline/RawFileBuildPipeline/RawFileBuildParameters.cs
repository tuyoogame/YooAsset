using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    public class RawFileBuildParameters : BuildParameters
    {
        /// <summary>
        /// 文件哈希值计算包含路径信息
        /// </summary>
        public bool IncludePathInHash = false;
    }
}