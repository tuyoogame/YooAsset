#if UNITY_2021_2_OR_NEWER
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建应用程序之前，把各个包裹产出的 link.xml 临时拷贝到 Assets 目录，
    /// 避免资源包内预制体引用的脚本被托管代码裁剪掉，构建结束后自动删除。
    /// </summary>
    /// <remarks>资源包需先于（或同时于）应用程序构建，否则找不到清单。</remarks>
    public class YooAssetPlayerBuildProcessor : BuildPlayerProcessor, IPostprocessBuildWithReport
    {
        /// <summary>
        /// 保留清单的文件名称
        /// </summary>
        private const string LinkFileName = "link.xml";

        /// <summary>
        /// 临时保留清单所在的文件夹
        /// </summary>
        /// <remarks>引擎只读取 Assets 目录下的保留清单，所以必须临时放置在工程内。</remarks>
        private const string TempFolderPath = "Assets/YooAssetTemp";

        /// <inheritdoc/>
        public override int callbackOrder => 0;

        /// <inheritdoc/>
        public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
        {
            // 上一次构建如果中途失败，不会触发构建结束回调，这里先清理再拷贝。
            RemoveTempFolder();

            string buildOutputRoot = BundleBuilderHelper.GetDefaultBuildOutputRoot();
            string platformDirectory = $"{buildOutputRoot}/{EditorUserBuildSettings.activeBuildTarget}";
            if (Directory.Exists(platformDirectory) == false)
                return;

            int copyCount = 0;
            foreach (string packageDirectory in Directory.GetDirectories(platformDirectory))
            {
                // 读取构建缓存目录下的那一份，它的路径不含版本号且始终是最新的。
                string sourceFilePath = $"{packageDirectory}/{YooAssetSettings.OutputFolderName}/{LinkFileName}";
                if (File.Exists(sourceFilePath) == false)
                    continue;

                // 引擎会读取 Assets 目录下所有的 link.xml 并取并集，所以每个包裹单独放一份即可不需要合并。
                string packageName = Path.GetFileName(packageDirectory);
                string destFilePath = $"{TempFolderPath}/{packageName}/{LinkFileName}";
                EditorFileUtility.CopyFile(sourceFilePath, destFilePath, true);
                copyCount++;
            }

            if (copyCount == 0)
                return;

            AssetDatabase.ImportAsset(TempFolderPath, ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            // 这里不走 BuildLogger：它的开关和日志文件由资源包构建管线初始化，构建应用程序时并不在那个上下文里。
            Debug.Log($"Copied {copyCount} link.xml files for player build : '{TempFolderPath}'.");
        }

        /// <inheritdoc/>
        /// <remarks>YooAsset.Editor 下也有一个同名的 BuildReport，所以这里必须写全名。</remarks>
        void IPostprocessBuildWithReport.OnPostprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            RemoveTempFolder();
        }

        /// <summary>
        /// 删除临时保留清单所在的文件夹
        /// </summary>
        private static void RemoveTempFolder()
        {
            if (Directory.Exists(TempFolderPath) == false)
                return;

            if (AssetDatabase.DeleteAsset(TempFolderPath))
                return;

            // 文件夹尚未被导入资源数据库时，回退到文件系统删除。
            EditorFileUtility.DeleteDirectory(TempFolderPath);
            string metaFilePath = TempFolderPath + ".meta";
            if (File.Exists(metaFilePath))
                File.Delete(metaFilePath);
        }
    }
}
#endif
