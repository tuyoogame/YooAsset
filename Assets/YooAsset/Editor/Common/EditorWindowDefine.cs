using System;
using System.Collections.ObjectModel;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器窗口相关的常量定义
    /// </summary>
    public static class EditorWindowDefine
    {
        private static readonly Type[] s_dockedWindowTypes =
        {
            typeof(BundleBuilderWindow),
            typeof(BundleCollectorWindow),
            typeof(BundleDebuggerWindow),
            typeof(BundleReporterWindow)
        };

        /// <summary>
        /// 停靠窗口类型集合
        /// </summary>
        public static readonly ReadOnlyCollection<Type> DockedWindowTypes = new ReadOnlyCollection<Type>(s_dockedWindowTypes);

        /// <summary>
        /// 获取停靠窗口类型数组的副本
        /// </summary>
        /// <returns>包含所有停靠窗口类型的新数组</returns>
        public static Type[] GetDockedWindowTypes()
        {
            return (Type[])s_dockedWindowTypes.Clone();
        }
    }
}
