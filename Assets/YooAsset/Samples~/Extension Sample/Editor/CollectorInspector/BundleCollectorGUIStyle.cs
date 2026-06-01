using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 扩展的 IMGUI 样式集合
    /// </summary>
    internal static class BundleCollectorGUIStyle
    {
        // 注意：GUIStyle 依赖 GUI 皮肤，不能在静态构造里创建，需延迟到绘制时初始化。
        private static GUIStyle _titleStyle;
        private static GUIStyle _fieldLabelStyle;
        private static GUIStyle _sectionStyle;

        /// <summary>
        /// 区块标题样式：保持正文字号，仅通过加粗和间距与内容区分。
        /// </summary>
        public static GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontStyle = FontStyle.Bold,
                        margin = new RectOffset(0, 0, 2, 4),
                    };
                }
                return _titleStyle;
            }
        }

        /// <summary>
        /// 字段名样式：保持正文字号。
        /// </summary>
        public static GUIStyle FieldLabelStyle
        {
            get
            {
                if (_fieldLabelStyle == null)
                {
                    _fieldLabelStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontStyle = FontStyle.Normal,
                    };
                }
                return _fieldLabelStyle;
            }
        }

        /// <summary>
        /// 分组卡片样式：基于 helpBox 增加内边距，让内容不贴边。
        /// </summary>
        public static GUIStyle SectionStyle
        {
            get
            {
                if (_sectionStyle == null)
                {
                    _sectionStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(6, 6, 5, 6),
                    };
                }
                return _sectionStyle;
            }
        }
    }
}
