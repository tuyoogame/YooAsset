using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 提供补丁包差异比对工具窗口
    /// </summary>
    public class PackageComparatorWindow : EditorWindow
    {
        static PackageComparatorWindow _thisInstance;
        
        [MenuItem("Tools/Patch Package Comparator", false, 103)]
        static void ShowWindow()
        {
            if (_thisInstance == null)
            {
                _thisInstance = EditorWindow.GetWindow(typeof(PackageComparatorWindow), false, "Patch Package Comparator", true) as PackageComparatorWindow;
                _thisInstance.minSize = new Vector2(800, 600);
            }
            _thisInstance.Show();
        }

        private string _manifestPath1 = string.Empty;
        private string _manifestPath2 = string.Empty;
        private readonly List<PackageBundle> _changeList = new List<PackageBundle>();
        private readonly List<PackageBundle> _newList = new List<PackageBundle>();
        private Vector2 _scrollPos1;
        private Vector2 _scrollPos2;

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Patch Package 1", GUILayout.MaxWidth(150)))
            {
                string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
                if (string.IsNullOrEmpty(resultPath))
                    return;
                _manifestPath1 = resultPath;
            }
            EditorGUILayout.LabelField(_manifestPath1);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select Patch Package 2", GUILayout.MaxWidth(150)))
            {
                string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
                if (string.IsNullOrEmpty(resultPath))
                    return;
                _manifestPath2 = resultPath;
            }
            EditorGUILayout.LabelField(_manifestPath2);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(_manifestPath1) == false && string.IsNullOrEmpty(_manifestPath2) == false)
            {
                if (GUILayout.Button("Compare Differences", GUILayout.MaxWidth(150)))
                {
                    try
                    {
                        ComparePackage(_changeList, _newList);
                    }
                    catch (System.Exception ex)
                    {
                        _changeList.Clear();
                        _newList.Clear();
                        Debug.LogError($"Failed to compare patch packages: {ex.Message}.");
                    }
                }
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(false))
            {
                int totalCount = _changeList.Count;
                EditorGUILayout.Foldout(true, $"Changed Bundles ( {totalCount} )");

                EditorGUI.indentLevel = 1;
                _scrollPos1 = EditorGUILayout.BeginScrollView(_scrollPos1);
                {
                    foreach (var bundle in _changeList)
                    {
                        EditorGUILayout.LabelField($"{bundle.BundleName} | {(bundle.FileSize / 1024)}K");
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel = 0;
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledScope(false))
            {
                int totalCount = _newList.Count;
                EditorGUILayout.Foldout(true, $"New Bundles ( {totalCount} )");

                EditorGUI.indentLevel = 1;
                _scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2);
                {
                    foreach (var bundle in _newList)
                    {
                        EditorGUILayout.LabelField($"{bundle.BundleName}");
                    }
                }
                EditorGUILayout.EndScrollView();
                EditorGUI.indentLevel = 0;
            }
        }

        private void ComparePackage(List<PackageBundle> changeList, List<PackageBundle> newList)
        {
            changeList.Clear();
            newList.Clear();

            // 加载基准补丁清单
            byte[] bytesData1 = FileUtility.ReadAllBytes(_manifestPath1);
            PackageManifest manifest1 = PackageManifestHelper.DeserializeManifestFromBinary(bytesData1, null); // TODO: 根据业务需求处理清单解密。

            // 加载待比对补丁清单
            byte[] bytesData2 = FileUtility.ReadAllBytes(_manifestPath2);
            PackageManifest manifest2 = PackageManifestHelper.DeserializeManifestFromBinary(bytesData2, null); // TODO: 根据业务需求处理清单解密。

            // 拷贝文件列表
            foreach (var bundle2 in manifest2.BundleList)
            {
                if (manifest1.TryGetPackageBundleByBundleName(bundle2.BundleName, out PackageBundle bundle1))
                {
                    if (bundle2.FileHash != bundle1.FileHash)
                    {
                        changeList.Add(bundle2);
                    }
                }
                else
                {
                    newList.Add(bundle2);
                }
            }

            // 按字母重新排序
            changeList.Sort((x, y) => string.Compare(x.BundleName, y.BundleName));
            newList.Sort((x, y) => string.Compare(x.BundleName, y.BundleName));

            Debug.Log("Package comparison completed.");
        }
    }
}
