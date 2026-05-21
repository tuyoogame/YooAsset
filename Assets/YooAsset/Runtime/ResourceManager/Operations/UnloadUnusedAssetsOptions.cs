
namespace YooAsset
{
    /// <summary>
    /// 卸载未使用资源的操作选项
    /// </summary>
    public readonly struct UnloadUnusedAssetsOptions
    {
        /// <summary>
        /// 最大循环迭代次数
        /// </summary>
        public int MaxLoopCount { get; }

        /// <summary>
        /// 创建卸载未使用资源的选项
        /// </summary>
        /// <param name="maxLoopCount">最大循环迭代次数</param>
        public UnloadUnusedAssetsOptions(int maxLoopCount)
        {
            MaxLoopCount = maxLoopCount;
        }
    }
}