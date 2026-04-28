using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 提供补丁包导入工具窗口
    /// </summary>
    public class PackageImporterWindow : EditorWindow
    {
        static PackageImporterWindow _thisInstance;

        [MenuItem("Tools/Patch Package Importer", false, 104)]
        static void ShowWindow()
        {
            if (_thisInstance == null)
            {
                _thisInstance = EditorWindow.GetWindow(typeof(PackageImporterWindow), false, "Patch Package Importer", true) as PackageImporterWindow;
                _thisInstance.minSize = new Vector2(800, 600);
            }
            _thisInstance.Show();
        }

        private string _manifestPath = string.Empty;
        private string _packageName = "DefaultPackage";

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            _packageName = EditorGUILayout.TextField("Package Name", _packageName);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Patch Package", GUILayout.MaxWidth(150)))
            {
                string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
                if (!string.IsNullOrEmpty(resultPath))
                    _manifestPath = resultPath;
            }
            EditorGUILayout.LabelField(_manifestPath);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(_manifestPath) == false)
            {
                if (GUILayout.Button("Import Patch Package (All Files)", GUILayout.MaxWidth(150)))
                {
                    if (string.IsNullOrEmpty(_packageName))
                    {
                        Debug.LogError("Package name is empty.");
                        return;
                    }

                    try
                    {
                        CopyPackageFiles(_manifestPath);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to import patch package '{_packageName}': {ex.Message}.");
                    }
                    finally
                    {
                        AssetDatabase.Refresh();
                    }
                }
            }
        }

        private void CopyPackageFiles(string manifestFilePath)
        {
            string sourceRoot = Path.GetDirectoryName(manifestFilePath);
            if (string.IsNullOrEmpty(sourceRoot))
                throw new DirectoryNotFoundException("Patch package directory does not exist.");

            string versionFileName = YooAssetConfiguration.GetPackageVersionFileName(_packageName);
            string versionSourcePath = Path.Combine(sourceRoot, versionFileName);
            string packageVersion = File.ReadAllText(versionSourcePath).Trim();
            string manifestFileName = YooAssetConfiguration.GetManifestBinaryFileName(_packageName, packageVersion);
            string hashFileName = YooAssetConfiguration.GetPackageHashFileName(_packageName, packageVersion);
            string selectedFileName = Path.GetFileName(manifestFilePath);
            if (selectedFileName != manifestFileName)
                throw new InvalidDataException($"Selected manifest file '{selectedFileName}' does not match expected manifest file '{manifestFileName}'.");

            string destRoot = Path.Combine(BundleBuilderHelper.GetStreamingAssetsRoot(), _packageName);

            // 清空旧目录
            EditorFileUtility.DeleteDirectory(destRoot);
            EditorFileUtility.CreateDirectory(destRoot);

            // 拷贝核心文件
            {
                string sourcePath = Path.Combine(sourceRoot, manifestFileName);
                string destPath = Path.Combine(destRoot, manifestFileName);
                EditorFileUtility.CopyFile(sourcePath, destPath, true);
            }
            {
                string sourcePath = Path.Combine(sourceRoot, hashFileName);
                string destPath = Path.Combine(destRoot, hashFileName);
                EditorFileUtility.CopyFile(sourcePath, destPath, true);
            }
            {
                string sourcePath = versionSourcePath;
                string destPath = Path.Combine(destRoot, versionFileName);
                EditorFileUtility.CopyFile(sourcePath, destPath, true);
            }

            // 加载补丁清单
            byte[] bytesData = FileUtility.ReadAllBytes(manifestFilePath);
            PackageManifest manifest = PackageManifestHelper.DeserializeManifestFromBinary(bytesData, null); // TODO: 根据业务需求处理清单解密。

            // 拷贝文件列表
            int fileCount = 0;
            foreach (var packageBundle in manifest.BundleList)
            {
                fileCount++;
                string fileName = packageBundle.GetFileName();
                string sourcePath = Path.Combine(sourceRoot, fileName);
                string destPath = Path.Combine(destRoot, fileName);
                EditorFileUtility.CopyFile(sourcePath, destPath, true);
            }

            Debug.Log($"Patch package copy completed. Copied {fileCount} bundle files.");
        }
    }
}