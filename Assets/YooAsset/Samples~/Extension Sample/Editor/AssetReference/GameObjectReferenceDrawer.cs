using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(GameObjectReference))]
public class GameObjectReferenceDrawer : PropertyDrawer
{
    private const float LineSpacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty packageNameProp = property.FindPropertyRelative("_packageName");
        SerializedProperty assetGUIDProp = property.FindPropertyRelative("_assetGUID");

        EditorGUI.BeginProperty(position, label, property);
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            Rect line = new Rect(position.x, position.y, position.width, lineHeight);

            // 绘制 PackageName
            packageNameProp.stringValue = EditorGUI.TextField(line, "Package Name", packageNameProp.stringValue);

            // 加载 GameObject
            string assetGUID = assetGUIDProp.stringValue;
            GameObject current = null;
            if (string.IsNullOrEmpty(assetGUID) == false)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                if (string.IsNullOrEmpty(assetPath) == false)
                    current = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            }

            // 绘制 GameObject
            line.y += lineHeight + LineSpacing;
            GameObject newAsset = (GameObject)EditorGUI.ObjectField(line, "Game Object", current, typeof(GameObject), false);
            if (newAsset != current)
            {
                if (newAsset == null)
                {
                    assetGUIDProp.stringValue = "";
                }
                else
                {
                    string newPath = AssetDatabase.GetAssetPath(newAsset);
                    if (string.IsNullOrEmpty(newPath) == false)
                        assetGUIDProp.stringValue = AssetDatabase.AssetPathToGUID(newPath);
                }
            }

            // 绘制 AssetGUID
            line.y += lineHeight + LineSpacing;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(line, "Asset GUID", assetGUIDProp.stringValue);
            EditorGUI.EndDisabledGroup();
        }
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + LineSpacing * 2;
    }
}
