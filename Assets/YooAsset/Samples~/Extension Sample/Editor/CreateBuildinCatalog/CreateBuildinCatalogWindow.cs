using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    public class CreateBuildinCatalogWindow : EditorWindow
    {
        static CreateBuildinCatalogWindow _thisInstance;

        [MenuItem("Tools/内置清单生成工具（Catalog）", false, 101)]
        static void ShowWindow()
        {
            if (_thisInstance == null)
            {
                _thisInstance = EditorWindow.GetWindow(typeof(CreateBuildinCatalogWindow), false, "内置清单生成工具", true) as CreateBuildinCatalogWindow;
                _thisInstance.minSize = new Vector2(800, 600);
            }
            _thisInstance.Show();
        }

        private string _directoryRoot = string.Empty;

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("选择内置资源目录", GUILayout.MaxWidth(150)))
            {
                string resultPath = EditorUtility.OpenFolderPanel("Find", "Assets/", "StreamingAssets");
                if (!string.IsNullOrEmpty(resultPath))
                    _directoryRoot = resultPath;
            }
            EditorGUILayout.LabelField(_directoryRoot);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(_directoryRoot) == false)
            {
                if (GUILayout.Button("生成Catalog文件", GUILayout.MaxWidth(150)))
                {
                    CreateCatalogFile(_directoryRoot);
                }
            }
        }

        private void CreateCatalogFile(string directoryRoot)
        {
            // 搜索所有Package目录
            List<string> packageRoots = GetPackageRoots(directoryRoot);
            foreach (var packageRoot in packageRoots)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(packageRoot);
                string packageName = directoryInfo.Name;
                try
                {
                    bool result = CatalogTools.CreateCatalogFile(null, packageName, packageRoot); //TODO 自行处理解密
                    if (result == false)
                    {
                        Debug.LogError($"Create package {packageName} catalog file failed ! See the detail error in console !");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Create package {packageName} catalog file failed ! {ex.Message}");
                }
            }
        }
        private List<string> GetPackageRoots(string rootPath)
        {
            // 检查目录是否存在
            if (Directory.Exists(rootPath) == false)
            {
                throw new DirectoryNotFoundException($"目录不存在: {rootPath}");
            }

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