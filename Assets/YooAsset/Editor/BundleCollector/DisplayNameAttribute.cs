using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器显示名字
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DisplayNameAttribute : Attribute
    {
        /// <summary>
        /// 编辑器显示名称
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// 创建 DisplayNameAttribute 实例
        /// </summary>
        /// <param name="name">显示名称</param>
        public DisplayNameAttribute(string name)
        {
            DisplayName = name;
        }
    }
}