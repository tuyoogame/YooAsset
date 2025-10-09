using System.Collections.Generic;

#if YOO_MACRO_SUPPORT
namespace YooAsset.Editor
{
    public class MacroDefine
    {
        /// <summary>
        /// YooAsset版本宏定义
        /// </summary>
        public static readonly List<string> Macros = new List<string>()
        {
            "YOO_ASSET_2",
            "YOO_ASSET_2_3",
            "YOO_ASSET_2_3_OR_NEWER",
        };
    }
}
#endif