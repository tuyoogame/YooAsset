using System.Diagnostics;

namespace YooAsset
{
    /// <summary>
    /// 自定义日志处理接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 输出普通日志
        /// </summary>
        void Log(string message);

        /// <summary>
        /// 输出警告日志
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// 输出错误日志
        /// </summary>
        void LogError(string message);

        /// <summary>
        /// 输出异常日志
        /// </summary>
        void LogException(System.Exception exception);
    }

    /// <summary>
    /// YooAsset内部日志系统
    /// </summary>
    internal static class YooLogger
    {
        /// <summary>
        /// 自定义日志处理器实例
        /// </summary>
        public static ILogger Current { get; set; }

        /// <summary>
        /// 输出调试日志（仅在 DEBUG 模式下生效）
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            if (Current != null)
            {
                Current.Log(message);
            }
            else
            {
                UnityEngine.Debug.Log(message);
            }
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        public static void LogWarning(string message)
        {
            if (Current != null)
            {
                Current.LogWarning(message);
            }
            else
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        public static void LogError(string message)
        {
            if (Current != null)
            {
                Current.LogError(message);
            }
            else
            {
                UnityEngine.Debug.LogError(message);
            }
        }

        /// <summary>
        /// 输出异常日志
        /// </summary>
        public static void LogException(System.Exception exception)
        {
            if (Current != null)
            {
                Current.LogException(exception);
            }
            else
            {
                UnityEngine.Debug.LogException(exception);
            }
        }
    }
}