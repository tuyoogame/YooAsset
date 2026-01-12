using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
    /// <summary>
    /// 录制开关
    /// </summary>
    public class ToggleRecord : Toggle
    {
        public new class UxmlFactory : UxmlFactory<ToggleRecord, UxmlTraits>
        {
        }

        private readonly VisualElement _checkbox;

        /// <summary>
        /// 创建录制开关实例
        /// </summary>
        public ToggleRecord()
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
                var icon = EditorGUIUtility.IconContent(UIElementsIcon.RecordOn).image as Texture2D;
                _checkbox.style.backgroundImage = icon;
            }
            else
            {
                var icon = EditorGUIUtility.IconContent(UIElementsIcon.RecordOff).image as Texture2D;
                _checkbox.style.backgroundImage = icon;
            }
        }
    }
}