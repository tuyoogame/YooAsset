
namespace YooAsset
{
    /// <summary>
    /// 文件校验结果
    /// </summary>
    internal enum EFileVerifyResult
    {
        /// <summary>
        /// 信息文件内容无效
        /// </summary>
        InfoFileInvalid = -24,

        /// <summary>
        /// 信息文件头标识不匹配
        /// </summary>
        InfoFileMagicError = -23,

        /// <summary>
        /// 信息文件版本不匹配
        /// </summary>
        InfoFileVersionError = -22,

        /// <summary>
        /// 信息文件不存在
        /// </summary>
        InfoFileNotExisted = -21,

        /// <summary>
        /// 数据文件内容无效
        /// </summary>
        DataFileInvalid = -15,

        /// <summary>
        /// 数据文件内容不足（小于正常大小）
        /// </summary>
        DataFileNotComplete = -14,

        /// <summary>
        /// 数据文件内容溢出（超过正常大小）
        /// </summary>
        DataFileOverflow = -13,

        /// <summary>
        /// 数据文件内容不匹配
        /// </summary>
        DataFileCrcError = -12,

        /// <summary>
        /// 数据文件不存在
        /// </summary>
        DataFileNotExisted = -11,

        /// <summary>
        /// 验证异常
        /// </summary>
        Exception = -1,

        /// <summary>
        /// 默认状态（校验未完成）
        /// </summary>
        None = 0,

        /// <summary>
        /// 验证成功
        /// </summary>
        Succeed = 1,
    }
}