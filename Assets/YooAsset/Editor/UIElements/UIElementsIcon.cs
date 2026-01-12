
namespace YooAsset.Editor
{
    /// <summary>
    /// Unity 编辑器内置图标名称常量集
    /// </summary>
    public static class UIElementsIcon
    {
        /// <summary>
        /// 录制开启图标
        /// </summary>
        public const string RecordOn = "d_Record On@2x";

        /// <summary>
        /// 录制关闭图标
        /// </summary>
        public const string RecordOff = "d_Record Off@2x";

#if UNITY_2019
        /// <summary>
        /// 折叠展开图标
        /// </summary>
        public const string FoldoutOn = "IN foldout on";

        /// <summary>
        /// 折叠收起图标
        /// </summary>
        public const string FoldoutOff = "IN foldout";
#else
        /// <summary>
        /// 折叠展开图标
        /// </summary>
        public const string FoldoutOn = "d_IN_foldout_on@2x";

        /// <summary>
        /// 折叠收起图标
        /// </summary>
        public const string FoldoutOff = "d_IN_foldout@2x";
#endif

        /// <summary>
        /// 可见性关闭图标
        /// </summary>
        public const string VisibilityToggleOff = "animationvisibilitytoggleoff@2x";

        /// <summary>
        /// 可见性开启图标
        /// </summary>
        public const string VisibilityToggleOn = "animationvisibilitytoggleon@2x";
    }
}