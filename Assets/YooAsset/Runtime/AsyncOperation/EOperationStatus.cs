
namespace YooAsset
{
    /// <summary>
    /// 异步操作状态枚举
    /// </summary>
    public enum EOperationStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        None,

        /// <summary>
        /// 处理中
        /// </summary>
        Processing,

        /// <summary>
        /// 已成功
        /// </summary>
        Succeeded,

        /// <summary>
        /// 已失败
        /// </summary>
        Failed,

#if YOOASSET_LEGACY_API
        /// <summary>
        /// v2.3 兼容别名
        /// </summary>
        [System.Obsolete("Use Succeeded instead.")]
        Succeed = Succeeded,
#endif
    }
}