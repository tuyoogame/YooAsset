using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器显示名字
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName;

        public DisplayNameAttribute(string name)
        {
            this.DisplayName = name;
        }
    }
}