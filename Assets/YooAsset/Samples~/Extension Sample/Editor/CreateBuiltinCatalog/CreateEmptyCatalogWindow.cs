using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 提供空资源清单生成工具窗口
    /// </summary>
    public class CreateEmptyCatalogWindow : EditorWindow
    {
        static CreateEmptyCatalogWindow _thisInstance;

        [MenuItem("Tools/Empty Catalog Generator", false, 102)]
        static void ShowWindow()
        {
            if (_thisInstance == null)
            {
                _thisInstance = EditorWindow.GetWindow(typeof(CreateEmptyCatalogWindow), false, "Empty Catalog Generator", true) as CreateEmptyCatalogWindow;
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
                if (GUILayout.Button("Generate Empty Catalog File", GUILayout.MaxWidth(150)))
                {
                    string outputPath = EditorDialogUtility.OpenFolderPanel("Output Directory", "Assets/");
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
                bool result = BuiltinCatalogHelper.CreateEmptyFile(_packageName, string.Empty, outputPath);
                if (result == false)
                {
                    Debug.LogError($"Failed to create a catalog file for package '{_packageName}'. See Console for details.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create a catalog file for package '{_packageName}': {ex.Message}.");
            }
        }
    }
}