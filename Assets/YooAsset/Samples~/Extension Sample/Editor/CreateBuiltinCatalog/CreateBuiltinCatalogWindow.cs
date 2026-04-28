using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 提供内置资源清单生成工具窗口
    /// </summary>
    public class CreateBuiltinCatalogWindow : EditorWindow
    {
        static CreateBuiltinCatalogWindow _thisInstance;

        [MenuItem("Tools/Builtin Catalog Generator", false, 101)]
        static void ShowWindow()
        {
            if (_thisInstance == null)
            {
                _thisInstance = EditorWindow.GetWindow(typeof(CreateBuiltinCatalogWindow), false, "Builtin Catalog Generator", true) as CreateBuiltinCatalogWindow;
                _thisInstance.minSize = new Vector2(800, 600);
            }
            _thisInstance.Show();
        }

        private string _directoryRoot = string.Empty;

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Builtin Resource Directory", GUILayout.MaxWidth(250)))
            {
                string resultPath = EditorUtility.OpenFolderPanel("Find", "Assets/", "StreamingAssets");
                if (!string.IsNullOrEmpty(resultPath))
                    _directoryRoot = resultPath;
            }
            EditorGUILayout.LabelField(_directoryRoot);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(_directoryRoot) == false)
            {
                if (GUILayout.Button("Generate Catalog File", GUILayout.MaxWidth(150)))
                {
                    CreateCatalogFile(_directoryRoot);
                }
            }
        }

        private void CreateCatalogFile(string directoryRoot)
        {
            // 检查目录是否存在
            if (Directory.Exists(directoryRoot) == false)
            {
                Debug.LogError("Selected directory does not exist.");
                return;
            }

            // 搜索所有Package目录
            List<string> packageRoots = GetPackageRoots(directoryRoot);
            foreach (var packageRoot in packageRoots)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(packageRoot);
                string packageName = directoryInfo.Name;
                try
                {
                    bool result = BuiltinCatalogHelper.CreateFile(null, packageName, packageRoot); // TODO: 根据业务需求处理清单解密。
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
        private List<string> GetPackageRoots(string rootPath)
        {
            // 搜索所有 .version 文件（包含子目录）
            string[] versionFiles = Directory.GetFiles(
                rootPath,
                "*.version",
                SearchOption.AllDirectories
            );

            // 提取文件所在目录路径并去重
            return versionFiles
                .Select(file => Path.GetDirectoryName(file))
                .Distinct()
                .ToList();
        }
    }
}