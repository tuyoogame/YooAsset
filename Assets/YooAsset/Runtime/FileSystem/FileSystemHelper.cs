using System;

namespace YooAsset
{
    internal static class FileSystemHelper
    {
        /// <summary>
        /// 将参数值安全转换为目标类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">待转换的参数值</param>
        /// <returns>转换后的值</returns>
        internal static T CastParameter<T>(string paramName, object value)
        {
            if (value is T typedValue)
                return typedValue;
            if (value == null)
                throw new ArgumentNullException(paramName);
            throw new ArgumentException(
                $"Failed to cast parameter, type '{value.GetType().Name}' does not match target type '{typeof(T).Name}'.",
                paramName);
        }
    }
}