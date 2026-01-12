using System.Reflection;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 通过反射创建自定义鼠标光标的工具类
    /// </summary>
    public static class UIElementsCursor
    {
        private static PropertyInfo s_defaultCursorId;
        private static PropertyInfo DefaultCursorId
        {
            get
            {
                if (s_defaultCursorId != null)
                    return s_defaultCursorId;

                s_defaultCursorId = typeof(UnityEngine.UIElements.Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
                return s_defaultCursorId;
            }
        }

        /// <summary>
        /// 创建指定类型的鼠标光标
        /// </summary>
        /// <param name="cursorType">光标类型</param>
        /// <returns>可用于 UIElements 样式的光标实例</returns>
        public static UnityEngine.UIElements.Cursor CreateCursor(MouseCursor cursorType)
        {
            var ret = (object)new UnityEngine.UIElements.Cursor();
            DefaultCursorId.SetValue(ret, (int)cursorType);
            return (UnityEngine.UIElements.Cursor)ret;
        }
    }
}