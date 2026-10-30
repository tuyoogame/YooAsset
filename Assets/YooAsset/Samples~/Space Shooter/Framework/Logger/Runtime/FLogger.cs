using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 框架日志门面
    /// </summary>
    public static class FLogger
    {
#if UNITY_EDITOR
        /// <summary>
        /// 重置静态数据
        /// </summary>
        /// <remarks>禁用域重载之后，静态数据不会自动重置。</remarks>
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticData()
        {
            s_data = new StaticData();
        }
#endif

        private const string SymbolEditor = "UNITY_EDITOR";
        private const string SymbolDevelopment = "DEVELOPMENT_BUILD";
        private const string SymbolForce = "LOG_ENABLE";
        
        private const string MessageFormat = "{0}";
        private const string TagFormat = "[{0}] {1}";

        private class StaticData
        {
            public bool LogEnabled = true;
            public LogType FilterLogType = LogType.Log;
            public ILogHandler Handler = UnityEngine.Debug.unityLogger.logHandler;
        }

        private static StaticData s_data = new StaticData();


        /// <summary>
        /// 日志总开关
        /// </summary>
        public static bool LogEnabled
        {
            get
            {
                return s_data.LogEnabled;
            }
            set
            {
                s_data.LogEnabled = value;
            }
        }

        /// <summary>
        /// 日志过滤级别
        /// </summary>
        /// <value>只有不低于该级别的日志才会输出</value>
        public static LogType FilterLogType
        {
            get
            {
                return s_data.FilterLogType;
            }
            set
            {
                if (value < LogType.Error || value > LogType.Exception)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "FilterLogType is not a valid LogType value.");

                s_data.FilterLogType = value;
            }
        }

        /// <summary>
        /// 日志输出后端，默认为引擎控制台。
        /// </summary>
        /// <remarks>
        /// <para>只有一个席位，需要同时输出到多个目标时自行实现一个分发用的后端。</para>
        /// </remarks>
        public static ILogHandler Handler
        {
            get
            {
                return s_data.Handler;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                s_data.Handler = value;
            }
        }

        /// <summary>
        /// 判断日志级别是否允许输出
        /// </summary>
        /// <param name="logType">要检查的日志级别</param>
        /// <returns>允许输出时返回 true；总开关关闭或不满足过滤级别时返回 false。</returns>
        /// <remarks>LogType 的枚举值越小级别越高，异常日志始终输出。</remarks>
        public static bool IsLogTypeAllowed(LogType logType)
        {
            if (LogEnabled == false)
                return false;
            if (logType == LogType.Exception)
                return true;
            if (FilterLogType == LogType.Exception)
                return false;

            return logType <= FilterLogType;
        }


        /// <summary>
        /// 输出普通日志
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <remarks>仅在编辑器、开发构建或定义 LOG_ENABLE 时编译。</remarks>
        [Conditional(SymbolEditor), Conditional(SymbolDevelopment), Conditional(SymbolForce)]
        public static void Log(object message)
        {
            if (IsLogTypeAllowed(LogType.Log) == false)
                return;

            s_data.Handler.LogFormat(LogType.Log, null, MessageFormat, GetString(message));
        }

        /// <summary>
        /// 输出普通日志并关联上下文对象
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <param name="context">控制台定位用的上下文对象，不进入消息文本。</param>
        /// <remarks>仅在编辑器、开发构建或定义 LOG_ENABLE 时编译。</remarks>
        [Conditional(SymbolEditor), Conditional(SymbolDevelopment), Conditional(SymbolForce)]
        public static void Log(object message, UnityEngine.Object context)
        {
            if (IsLogTypeAllowed(LogType.Log) == false)
                return;

            s_data.Handler.LogFormat(LogType.Log, context, MessageFormat, GetString(message));
        }

        /// <summary>
        /// 输出带标签的普通日志
        /// </summary>
        /// <param name="tag">日志标签</param>
        /// <param name="message">日志内容</param>
        /// <remarks>仅在编辑器、开发构建或定义 LOG_ENABLE 时编译。</remarks>
        [Conditional(SymbolEditor), Conditional(SymbolDevelopment), Conditional(SymbolForce)]
        public static void LogTag(string tag, object message)
        {
            if (IsLogTypeAllowed(LogType.Log) == false)
                return;

            s_data.Handler.LogFormat(LogType.Log, null, TagFormat, tag, GetString(message));
        }

        /// <summary>
        /// 输出带标签的普通日志并关联上下文对象
        /// </summary>
        /// <param name="tag">日志标签</param>
        /// <param name="message">日志内容</param>
        /// <param name="context">控制台定位用的上下文对象，不进入消息文本。</param>
        /// <remarks>仅在编辑器、开发构建或定义 LOG_ENABLE 时编译。</remarks>
        [Conditional(SymbolEditor), Conditional(SymbolDevelopment), Conditional(SymbolForce)]
        public static void LogTag(string tag, object message, UnityEngine.Object context)
        {
            if (IsLogTypeAllowed(LogType.Log) == false)
                return;

            s_data.Handler.LogFormat(LogType.Log, context, TagFormat, tag, GetString(message));
        }

        /// <summary>
        /// 按格式模板输出普通日志
        /// </summary>
        /// <param name="format">交给后端的格式模板</param>
        /// <param name="args">与模板对应的格式化参数</param>
        /// <remarks>仅在编辑器、开发构建或定义 LOG_ENABLE 时编译。校验发生在级别过滤之前。</remarks>
        [Conditional(SymbolEditor), Conditional(SymbolDevelopment), Conditional(SymbolForce)]
        public static void LogFormat(string format, params object[] args)
        {
            CheckFormatArguments(format, args);
            if (IsLogTypeAllowed(LogType.Log) == false)
                return;

            s_data.Handler.LogFormat(LogType.Log, null, format, args);
        }

        /// <summary>
        /// 输出警告日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Warning(object message)
        {
            if (IsLogTypeAllowed(LogType.Warning) == false)
                return;

            s_data.Handler.LogFormat(LogType.Warning, null, MessageFormat, GetString(message));
        }

        /// <summary>
        /// 输出警告日志并关联上下文对象
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <param name="context">控制台定位用的上下文对象，不进入消息文本。</param>
        public static void Warning(object message, UnityEngine.Object context)
        {
            if (IsLogTypeAllowed(LogType.Warning) == false)
                return;

            s_data.Handler.LogFormat(LogType.Warning, context, MessageFormat, GetString(message));
        }

        /// <summary>
        /// 输出带标签的警告日志
        /// </summary>
        /// <param name="tag">日志标签</param>
        /// <param name="message">日志内容</param>
        public static void WarningTag(string tag, object message)
        {
            if (IsLogTypeAllowed(LogType.Warning) == false)
                return;

            s_data.Handler.LogFormat(LogType.Warning, null, TagFormat, tag, GetString(message));
        }

        /// <summary>
        /// 输出带标签的警告日志并关联上下文对象
        /// </summary>
        /// <param name="tag">日志标签</param>
        /// <param name="message">日志内容</param>
        /// <param name="context">控制台定位用的上下文对象，不进入消息文本。</param>
        public static void WarningTag(string tag, object message, UnityEngine.Object context)
        {
            if (IsLogTypeAllowed(LogType.Warning) == false)
                return;

            s_data.Handler.LogFormat(LogType.Warning, context, TagFormat, tag, GetString(message));
        }

        /// <summary>
        /// 按格式模板输出警告日志
        /// </summary>
        /// <param name="format">交给后端的格式模板</param>
        /// <param name="args">与模板对应的格式化参数</param>
        /// <remarks>校验发生在级别过滤之前。</remarks>
        public static void WarningFormat(string format, params object[] args)
        {
            CheckFormatArguments(format, args);
            if (IsLogTypeAllowed(LogType.Warning) == false)
                return;

            s_data.Handler.LogFormat(LogType.Warning, null, format, args);
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Error(object message)
        {
            if (IsLogTypeAllowed(LogType.Error) == false)
                return;

            s_data.Handler.LogFormat(LogType.Error, null, MessageFormat, GetString(message));
        }

        /// <summary>
        /// 输出错误日志并关联上下文对象
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <param name="context">控制台定位用的上下文对象，不进入消息文本。</param>
        public static void Error(object message, UnityEngine.Object context)
        {
            if (IsLogTypeAllowed(LogType.Error) == false)
                return;

            s_data.Handler.LogFormat(LogType.Error, context, MessageFormat, GetString(message));
        }

        /// <summary>
        /// 输出带标签的错误日志
        /// </summary>
        /// <param name="tag">日志标签</param>
        /// <param name="message">日志内容</param>
        public static void ErrorTag(string tag, object message)
        {
            if (IsLogTypeAllowed(LogType.Error) == false)
                return;

            s_data.Handler.LogFormat(LogType.Error, null, TagFormat, tag, GetString(message));
        }

        /// <summary>
        /// 输出带标签的错误日志并关联上下文对象
        /// </summary>
        /// <param name="tag">日志标签</param>
        /// <param name="message">日志内容</param>
        /// <param name="context">控制台定位用的上下文对象，不进入消息文本。</param>
        public static void ErrorTag(string tag, object message, UnityEngine.Object context)
        {
            if (IsLogTypeAllowed(LogType.Error) == false)
                return;

            s_data.Handler.LogFormat(LogType.Error, context, TagFormat, tag, GetString(message));
        }

        /// <summary>
        /// 按格式模板输出错误日志
        /// </summary>
        /// <param name="format">交给后端的格式模板</param>
        /// <param name="args">与模板对应的格式化参数</param>
        /// <remarks>校验发生在级别过滤之前。</remarks>
        public static void ErrorFormat(string format, params object[] args)
        {
            CheckFormatArguments(format, args);
            if (IsLogTypeAllowed(LogType.Error) == false)
                return;

            s_data.Handler.LogFormat(LogType.Error, null, format, args);
        }

        /// <summary>
        /// 条件不成立时输出断言日志
        /// </summary>
        /// <param name="condition">断言条件，为 true 时不输出。</param>
        /// <param name="message">断言失败时的日志内容</param>
        /// <remarks>仅在编辑器、开发构建或定义 LOG_ENABLE 时编译。</remarks>
        [Conditional(SymbolEditor), Conditional(SymbolDevelopment), Conditional(SymbolForce)]
        public static void Assert(bool condition, object message)
        {
            if (condition)
                return;
            if (IsLogTypeAllowed(LogType.Assert) == false)
                return;

            s_data.Handler.LogFormat(LogType.Assert, null, MessageFormat, GetString(message));
        }

        private static void CheckFormatArguments(string format, object[] args)
        {
            if (format == null)
                throw new ArgumentNullException(nameof(format));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
        }
        private static string GetString(object message)
        {
            if (message == null)
                return "Null";

            if (message is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return message.ToString();
        }
    }
}
