using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 扩展的 IMGUI 绘制辅助方法集合
    /// </summary>
    internal static class BundleCollectorGUIDraw
    {
        /// <summary>
        /// 绘制区块标题
        /// </summary>
        /// <param name="title">标题文本</param>
        public static void DrawSectionTitle(string title)
        {
            EditorGUILayout.LabelField(title, BundleCollectorGUIStyle.TitleStyle);
            EditorGUILayout.Space(2);
        }

        /// <summary>
        /// 绘制一个带字段名的只读文本字段
        /// </summary>
        /// <param name="label">字段名</param>
        /// <param name="value">字段值</param>
        /// <param name="disabled">是否置灰整行</param>
        public static void DrawLabelField(string label, string value, bool disabled = false)
        {
            using (new EditorGUI.DisabledScope(disabled))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(label, BundleCollectorGUIStyle.FieldLabelStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(value) ? "-" : value);
                }
            }
        }

        /// <summary>
        /// 绘制一个带字段名的延迟文本输入框
        /// </summary>
        /// <param name="label">字段名</param>
        /// <param name="value">当前文本值</param>
        /// <returns>新的文本值</returns>
        public static string DrawTextField(string label, string value)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, BundleCollectorGUIStyle.FieldLabelStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
                return EditorGUILayout.DelayedTextField(value);
            }
        }

        /// <summary>
        /// 绘制一个带字段名的下拉框
        /// </summary>
        /// <param name="label">字段名</param>
        /// <param name="index">当前选项索引</param>
        /// <param name="displayedOptions">下拉选项</param>
        /// <returns>新的选项索引</returns>
        public static int DrawPopupField(string label, int index, string[] displayedOptions)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, BundleCollectorGUIStyle.FieldLabelStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
                return EditorGUILayout.Popup(index, displayedOptions);
            }
        }

        /// <summary>
        /// 绘制规则选择行
        /// </summary>
        /// <param name="label">字段标签</param>
        /// <param name="displayNames">显示名称列表</param>
        /// <param name="currentIndex">当前选中索引</param>
        /// <param name="newIndex">新选中的索引</param>
        /// <returns>规则索引发生变化返回 true</returns>
        public static bool TryDrawRuleSelection(string label, string[] displayNames, int currentIndex, out int newIndex)
        {
            newIndex = -1;
            if (displayNames == null || displayNames.Length == 0)
            {
                using (new EditorGUI.DisabledScope(true)) DrawPopupField(label, 0, new[] { "<None>" });
                return false;
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, displayNames.Length - 1);
            newIndex = DrawPopupField(label, currentIndex, displayNames);
            return newIndex != currentIndex;
        }
    }
}
