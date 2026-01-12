
namespace YooAsset
{
    /// <summary>
    /// 诊断命令类型
    /// </summary>
    internal enum EDiagnosticCommandType
    {
        /// <summary>
        /// 未指定命令
        /// </summary>
        None = 0,

        /// <summary>
        /// 采样一次
        /// </summary>
        SampleOnce = 1,

        /// <summary>
        /// 持续采样
        /// </summary>
        AutoSampling = 2,
    }
}