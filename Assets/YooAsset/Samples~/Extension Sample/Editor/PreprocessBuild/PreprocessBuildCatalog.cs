using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset.Editor;

namespace YooAsset
{
    /// <summary>
    /// 在应用构建前生成内置资源清单
    /// </summary>
    public class PreprocessBuildCatalog : UnityEditor.Build.IPreprocessBuildWithReport
    {
        /// <summary>
        /// 构建预处理回调顺序
        /// </summary>
        public int callbackOrder { get { return 0; } }

        /// <inheritdoc/>
        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            YooLogger.Log("Starting catalog file generation.");

            string rootPath = BundleBuilderHelper.GetStreamingAssetsRoot();
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if (rootDirectory.Exists == false)
            {
                Debug.LogWarning("StreamingAssets root directory does not exist.");
                return;
            }

            // 搜索所有Package目录
            DirectoryInfo[] subDirectories = rootDirectory.GetDirectories();
            foreach (var subDirectory in subDirectories)
            {
                string packageName = subDirectory.Name;
                string packageDirectory = subDirectory.FullName;
                try
                {
                    bool result = BuiltinCatalogHelper.CreateFile(null, packageName, packageDirectory); // TODO: 根据业务需求处理清单解密。
                    if (result == false)
                    {
                        Debug.LogError($"Failed to create a catalog file for package '{packageName}'. See Console for details.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to create a catalog file for package '{packageName}': {ex.Message}.");
                }
            }
        }
    }
}