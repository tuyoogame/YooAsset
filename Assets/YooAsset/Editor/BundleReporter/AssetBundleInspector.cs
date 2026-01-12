using System.IO;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// AssetBundle 资源的自定义 Inspector 面板
    /// </summary>
    [CustomEditor(typeof(AssetBundle))]
    internal class AssetBundleInspector : UnityEditor.Editor
    {
        private bool _pathFoldout = false;
        private bool _advancedFoldout = false;

        public override void OnInspectorGUI()
        {
            AssetBundle bundle = target as AssetBundle;

            using (new EditorGUI.DisabledScope(true))
            {
                var leftStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
                leftStyle.alignment = TextAnchor.UpperLeft;
                GUILayout.Label(new GUIContent("Name: " + bundle.name), leftStyle);

                var assetNames = bundle.GetAllAssetNames();
                _pathFoldout = EditorGUILayout.Foldout(_pathFoldout, "Source Asset Paths");
                if (_pathFoldout)
                {
                    EditorGUI.indentLevel++;
                    foreach (var asset in assetNames)
                        EditorGUILayout.LabelField(asset);
                    EditorGUI.indentLevel--;
                }

                _advancedFoldout = EditorGUILayout.Foldout(_advancedFoldout, "Advanced Data");
            }

            if (_advancedFoldout)
            {
                EditorGUI.indentLevel++;
                base.OnInspectorGUI();
                EditorGUI.indentLevel--;
            }
        }
    }
}
