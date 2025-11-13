using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    public class CreateEmptyCatalogWindow : EditorWindow
    {
        static CreateEmptyCatalogWindow _thisInstance;

        [MenuItem("Tools/空清单生成工具（Catalog）", false, 102)]
        static void ShowWindow()
        {
            if (_thisInstance == null)
            {
                _thisInstance = EditorWindow.GetWindow(typeof(CreateEmptyCatalogWindow), false, "空清单生成工具", true) as CreateEmptyCatalogWindow;
                _thisInstance.minSize = new Vector2(800, 600);
            }
            _thisInstance.Show();
        }

        private string _packageName = string.Empty;

        private void OnGUI()
        {
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            _packageName = EditorGUILayout.TextField("Package Name", _packageName);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(_packageName) == false)
            {
                if (GUILayout.Button("生成空的Catalog文件", GUILayout.MaxWidth(150)))
                {
                    string outputPath = EditorTools.OpenFolderPanel("输出目录", "Assets/");
                    if (string.IsNullOrEmpty(outputPath) == false)
                    {
                        CreateEmptyCatalogFile(outputPath);
                    }
                }
            }
        }

        private void CreateEmptyCatalogFile(string outputPath)
        {
            try
            {
                bool result = CatalogTools.CreateEmptyCatalogFile(_packageName, string.Empty, outputPath);
                if (result == false)
                {
                    Debug.LogError($"Create package {_packageName} catalog file failed ! See the detail error in console !");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Create package {_packageName} catalog file failed ! {ex.Message}");
            }
        }
    }
}