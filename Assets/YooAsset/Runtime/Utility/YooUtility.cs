using System;
using System.Diagnostics;

namespace YooAsset
{
    /// <summary>
    /// 自定义日志处理
    /// </summary>
    public interface ILogger
    {
        void Log(string message);
        void Warning(string message);
        void Error(string message);
        void Exception(System.Exception exception);
    }

    internal static class YooLogger
    {
        public static ILogger Logger = null;

        /// <summary>
        /// 日志
        /// </summary>
        [Conditional("DEBUG")]
        public static void Log(string info)
        {
            if (Logger != null)
            {
                Logger.Log(GetTime() + info);
            }
            else
            {
                UnityEngine.Debug.Log(GetTime() + info);
            }
        }

        /// <summary>
        /// 警告
        /// </summary>
        public static void Warning(string info)
        {
            if (Logger != null)
            {
                Logger.Warning(GetTime() + info);
            }
            else
            {
                UnityEngine.Debug.LogWarning(GetTime() + info);
            }
        }

        /// <summary>
        /// 错误
        /// </summary>
        public static void Error(string info)
        {
            if (Logger != null)
            {
                Logger.Error(GetTime() + info);
            }
            else
            {
                UnityEngine.Debug.LogError(GetTime() + info);
            }
        }

        private static string GetTime()
        {
            return $"[YooAsset]:[{DateTime.Now:HH:mm:ss.fff}]:";
        }

        /// <summary>
        /// 异常
        /// </summary>
        public static void Exception(System.Exception exception)
        {
            if (Logger != null)
            {
                Logger.Exception(exception);
            }
            else
            {
                UnityEngine.Debug.LogException(exception);
            }
        }
    }
}
