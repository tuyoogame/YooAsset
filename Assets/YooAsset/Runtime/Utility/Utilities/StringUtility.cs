using System;
using System.Text;

namespace YooAsset
{
    /// <summary>
    /// 字符串工具类
    /// </summary>
    internal static class StringUtility
    {
        [ThreadStatic]
        private static StringBuilder t_cachedBuilder;

        [ThreadStatic]
        private static bool t_inUse;

        // 重入保护：Format 的参数可能在 ToString() 中再次调用 Format，
        // 若复用同一个 StringBuilder 会导致外层内容被清空而静默返回错误数据。
        // 当检测到嵌套调用时，退化为创建新实例以保证正确性。
        private static StringBuilder AcquireBuilder()
        {
            if (t_inUse)
                return new StringBuilder(256);

            if (t_cachedBuilder == null)
                t_cachedBuilder = new StringBuilder(2048);

            t_cachedBuilder.Length = 0;
            t_inUse = true;
            return t_cachedBuilder;
        }

        private static void ReleaseBuilder(StringBuilder sb)
        {
            if (sb == t_cachedBuilder)
                t_inUse = false;
        }

        /// <summary>
        /// 无GC格式化字符串
        /// </summary>
        public static string Format(string format, object arg0)
        {
            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(nameof(format));

            var sb = AcquireBuilder();
            try
            {
                sb.AppendFormat(format, arg0);
                return sb.ToString();
            }
            finally
            {
                ReleaseBuilder(sb);
            }
        }

        /// <summary>
        /// 无GC格式化字符串
        /// </summary>
        public static string Format(string format, object arg0, object arg1)
        {
            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(nameof(format));

            var sb = AcquireBuilder();
            try
            {
                sb.AppendFormat(format, arg0, arg1);
                return sb.ToString();
            }
            finally
            {
                ReleaseBuilder(sb);
            }
        }

        /// <summary>
        /// 无GC格式化字符串
        /// </summary>
        public static string Format(string format, object arg0, object arg1, object arg2)
        {
            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(nameof(format));

            var sb = AcquireBuilder();
            try
            {
                sb.AppendFormat(format, arg0, arg1, arg2);
                return sb.ToString();
            }
            finally
            {
                ReleaseBuilder(sb);
            }
        }

        /// <summary>
        /// 无GC格式化字符串
        /// </summary>
        public static string Format(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format))
                throw new ArgumentNullException(nameof(format));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var sb = AcquireBuilder();
            try
            {
                sb.AppendFormat(format, args);
                return sb.ToString();
            }
            finally
            {
                ReleaseBuilder(sb);
            }
        }
    }
}