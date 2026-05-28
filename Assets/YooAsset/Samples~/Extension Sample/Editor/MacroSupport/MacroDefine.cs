using System.Collections.Generic;

#if YOOASSET_MACRO_SUPPORT
namespace YooAsset.Editor
{
    /// <summary>
    /// 提供 YooAsset 版本相关的脚本宏定义
    /// </summary>
    public static class MacroDefine
    {
        /// <summary>
        /// YooAsset 版本宏定义集合
        /// </summary>
        public static IReadOnlyList<string> Macros { get; } = new List<string>()
        {
            "YOOASSET_3",
            "YOOASSET_3_0",
            "YOOASSET_3_0_OR_NEWER",
        };
    }
}
#endif