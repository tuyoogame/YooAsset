using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 显示开关（眼睛图标）
    /// </summary>
#if UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    public partial class ToggleDisplay : Toggle
    {
#if !UNITY_6000_0_OR_NEWER
        public new class UxmlFactory : UxmlFactory<ToggleDisplay, UxmlTraits>
        {
        }
#endif

        private readonly VisualElement _checkbox;

        /// <summary>
        /// 创建显示开关实例
        /// </summary>
        public ToggleDisplay()
        {
            _checkbox = this.Q<VisualElement>("unity-checkmark");
            RefreshIcon();
        }
        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RefreshIcon();
        }

#if UNITY_2021_3_OR_NEWER
        protected override void ToggleValue()
        {
            base.ToggleValue();
            RefreshIcon();
        }
#endif

        private void RefreshIcon()
        {
            if (this.value)
            {
                var icon = EditorGUIUtility.IconContent(UIElementsIcon.VisibilityToggleOff).image as Texture2D;
                _checkbox.style.backgroundImage = icon;
            }
            else
            {
                var icon = EditorGUIUtility.IconContent(UIElementsIcon.VisibilityToggleOn).image as Texture2D;
                _checkbox.style.backgroundImage = icon;
            }
        }
    }
}
