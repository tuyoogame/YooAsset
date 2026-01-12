using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// UIElements 通用工具方法集
    /// </summary>
    public static class UIElementsTools
    {
        /// <summary>
        /// 设置元素显隐
        /// </summary>
        /// <param name="element">目标 UI 元素</param>
        /// <param name="visible">是否可见</param>
        public static void SetElementVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>
        /// 设置元素内部 Label 的最小宽度
        /// </summary>
        /// <param name="element">包含 Label 的父元素</param>
        /// <param name="minWidth">最小宽度（像素）</param>
        public static void SetElementLabelMinWidth(VisualElement element, int minWidth)
        {
            var label = element.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = minWidth;
            }
        }

        /// <summary>
        /// 设置 ObjectField 的显示文本为资源路径
        /// </summary>
        /// <param name="objectField">目标 ObjectField 元素</param>
        public static void SetObjectFieldShowPath(ObjectField objectField)
        {
            string labelClassName = "unity-object-field-display__label";
            var nameLabel = objectField.Q<Label>(className: labelClassName);
            if (nameLabel == null)
                return;

            objectField.RegisterValueChangedCallback(evt =>
            {
                Object obj = evt.newValue;
                if (obj == null)
                {
                    nameLabel.text = "None (Object)";
                    return;
                }

                // 获取资源路径（仅适用于项目资源）
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path) == false)
                {
                    nameLabel.text = path;
                }
                else
                {
                    nameLabel.text = obj.name;
                }
            });
        }

        /// <summary>
        /// 刷新 ObjectField 的资源路径显示文本
        /// </summary>
        /// <param name="objectField">目标 ObjectField 元素</param>
        public static void RefreshObjectFieldShowPath(ObjectField objectField)
        {
            string labelClassName = "unity-object-field-display__label";
            var nameLabel = objectField.Q<Label>(className: labelClassName);
            if (nameLabel == null)
                return;

            Object obj = objectField.value;
            if (obj == null)
            {
                nameLabel.text = "None (Object)";
                return;
            }

            // 获取资源路径（仅适用于项目资源）
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) == false)
            {
                nameLabel.text = path;
            }
            else
            {
                nameLabel.text = obj.name;
            }
        }

        /// <summary>
        /// 设置工具栏按钮的图标
        /// </summary>
        /// <param name="element">目标 ToolbarButton 元素</param>
        /// <param name="iconName">Unity 内置图标名称</param>
        public static void SetToolbarButtonIcon(ToolbarButton element, string iconName)
        {
            var image = EditorGUIUtility.IconContent(iconName).image as Texture2D;
            element.style.backgroundImage = image;
            element.text = string.Empty;
        }

        /// <summary>
        /// 将两个面板重组为垂直分屏布局
        /// </summary>
        /// <param name="root">容纳分屏的父元素</param>
        /// <param name="panelA">上方面板</param>
        /// <param name="panelB">下方面板</param>
        public static void SplitVerticalPanel(VisualElement root, VisualElement panelA, VisualElement panelB)
        {
#if UNITY_2020_3_OR_NEWER
            root.Remove(panelA);
            root.Remove(panelB);

            var splitView = new TwoPaneSplitView();
            splitView.fixedPaneInitialDimension = 300;
            splitView.orientation = TwoPaneSplitViewOrientation.Vertical;
            splitView.contentContainer.Add(panelA);
            splitView.contentContainer.Add(panelB);
            root.Add(splitView);
#endif
        }
    }
}