using UnityEngine;

namespace YooAsset
{
    /// <summary>
    /// YooAsset 全局配置
    /// </summary>
    [CreateAssetMenu(fileName = "YooAssetSettings", menuName = "YooAsset/Create YooAsset Settings")]
    internal class YooAssetSettings : ScriptableObject
    {
        /// <summary>
        /// 资源包裹的根文件夹名称
        /// </summary>
        /// <remarks>设为空则直接使用平台默认路径</remarks>
        public string YooFolderName = "yoo";

        /// <summary>
        /// 资源包裹的文件名称前缀
        /// </summary>
        /// <remarks>设为空则不添加前缀</remarks>
        public string PackageFilePrefix = string.Empty;
        

        /// <summary>
        /// 构建输出文件夹名称
        /// </summary>
        public const string OutputFolderName = "OutputCache";

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PathUtility.ContainsInvalidFileNameChars(YooFolderName))
                Debug.LogError($"YooFolderName '{YooFolderName}' contains invalid file name characters.");
            if (PathUtility.ContainsInvalidFileNameChars(PackageFilePrefix))
                Debug.LogError($"PackageFilePrefix '{PackageFilePrefix}' contains invalid file name characters.");
        }
#endif
    }
}