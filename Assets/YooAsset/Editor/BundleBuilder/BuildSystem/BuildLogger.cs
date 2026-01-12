using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建流程日志系统，支持日志输出到文件和 Unity 控制台
    /// </summary>
    internal static class BuildLogger
    {
        private const int MaxLogBufferSize = 1024 * 1024 * 10; //10MB

        private static bool s_enableLog = true;
        private static bool s_logBufferOverflow = false;
        private static string s_logFilePath;

        private static readonly object s_lockObj = new object();
        private static readonly StringBuilder s_logBuilder = new StringBuilder(MaxLogBufferSize);

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        /// <param name="enableLog">是否启用日志记录</param>
        /// <param name="logFilePath">日志文件的输出路径</param>
        public static void InitLogger(bool enableLog, string logFilePath)
        {
            s_enableLog = enableLog;
            s_logFilePath = logFilePath;
            s_logBuilder.Clear();
            s_logBufferOverflow = false;

            if (s_enableLog)
            {
                if (string.IsNullOrEmpty(s_logFilePath))
                    throw new ArgumentException("Log file path is null or empty.", nameof(logFilePath));

                Debug.Log($"Logger initialized at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.");
            }
        }

        /// <summary>
        /// 关闭日志系统
        /// </summary>
        public static void Shutdown()
        {
            if (s_enableLog)
            {
                lock (s_lockObj)
                {
                    try
                    {
                        if (File.Exists(s_logFilePath))
                            File.Delete(s_logFilePath);

                        FileUtility.EnsureParentDirectoryExists(s_logFilePath);
                        File.WriteAllText(s_logFilePath, s_logBuilder.ToString(), Encoding.UTF8);
                        s_logBuilder.Clear();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to write log file: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Log(string message)
        {
            if (s_enableLog)
            {
                WriteLog("INFO", message);
                Debug.Log(message);
            }
        }

        /// <summary>
        /// 记录警告级别日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Warning(string message)
        {
            Debug.LogWarning(message);
            if (s_enableLog)
            {
                WriteLog("WARN", message);
            }
        }

        /// <summary>
        /// 记录错误级别日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Error(string message)
        {
            Debug.LogError(message);
            if (s_enableLog)
            {
                WriteLog("ERROR", message);
            }
        }

        /// <summary>
        /// 格式化错误码和消息为统一的错误字符串
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="message">错误描述</param>
        /// <returns>格式化后的错误消息字符串</returns>
        public static string GetErrorMessage(ErrorCode code, string message)
        {
            return $"[ErrorCode{(int)code}] {message}";
        }

        private static void WriteLog(string level, string message)
        {
            lock (s_lockObj)
            {
                if (s_logBuilder.Length >= MaxLogBufferSize)
                {
                    if (s_logBufferOverflow == false)
                    {
                        s_logBufferOverflow = true;
                        Debug.LogWarning($"Build log buffer exceeded {EditorUtility.FormatBytes(MaxLogBufferSize)} limit. Further log entries will be discarded.");
                    }
                    return;
                }
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
                s_logBuilder.AppendLine(logEntry);
            }
        }
    }
}