using System;

namespace YooAsset
{
    /// <summary>
    /// 诊断系统的常量定义
    /// </summary>
    internal static class DiagnosticSystemConsts
    {
        /// <summary>
        /// 通信协议版本号
        /// </summary>
        public const string ProtocolVersion = "1.0";

        /// <summary>
        /// Player 向 Editor 发送消息的标识符
        /// </summary>
        public static readonly Guid PlayerToEditorMessageId = new Guid("e34a5702dd353724aa315fb8011f08c3");

        /// <summary>
        /// Editor 向 Player 发送消息的标识符
        /// </summary>
        public static readonly Guid EditorToPlayerMessageId = new Guid("4d1926c9df5b052469a1c63448b7609a");

        /// <summary>
        /// 自动采样开启参数值
        /// </summary>
        public const string AutoSamplingOpen = "open";
    }
}