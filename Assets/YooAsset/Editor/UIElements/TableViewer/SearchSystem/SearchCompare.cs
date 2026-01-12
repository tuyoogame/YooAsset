using System;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 指定标题的数值比较搜索命令
    /// </summary>
    public class SearchCompare : ISearchCommand
    {
        /// <inheritdoc/>
        public string SearchTag => HeaderTitle;

        /// <summary>
        /// 目标列的标题名称
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// 用于比较的数值字符串
        /// </summary>
        public string CompareValue { get; set; }

        /// <summary>
        /// 比较运算符，支持大于、大于等于、小于、小于等于、等于、不等于
        /// </summary>
        public string CompareOperator { get; set; }

        // 整型数值
        private bool _isConvertedInteger = false;
        private bool _isValidInteger = false;
        private long _cachedInteger = 0;
        private bool TryGetIntegerValue(out long value)
        {
            if (_isConvertedInteger == false)
            {
                _isConvertedInteger = true;
                _isValidInteger = long.TryParse(CompareValue, out _cachedInteger);
                if (_isValidInteger == false)
                    Debug.LogWarning($"Failed to parse integer from CompareValue: '{CompareValue}'.");
            }
            value = _cachedInteger;
            return _isValidInteger;
        }

        // 浮点数值
        private bool _isConvertedSingle = false;
        private bool _isValidSingle = false;
        private double _cachedSingle = 0;
        private bool TryGetSingleValue(out double value)
        {
            if (_isConvertedSingle == false)
            {
                _isConvertedSingle = true;
                _isValidSingle = double.TryParse(CompareValue, out _cachedSingle);
                if (_isValidSingle == false)
                    Debug.LogWarning($"Failed to parse double from CompareValue: '{CompareValue}'.");
            }
            value = _cachedSingle;
            return _isValidSingle;
        }

        /// <summary>
        /// 将整数值与目标进行比较
        /// </summary>
        /// <param name="value">待比较的整数值</param>
        /// <returns>满足比较条件时返回 true</returns>
        public bool CompareTo(long value)
        {
            if (TryGetIntegerValue(out long target) == false)
                return false;

            if (CompareOperator == ">")
                return value > target;
            else if (CompareOperator == ">=")
                return value >= target;
            else if (CompareOperator == "<")
                return value < target;
            else if (CompareOperator == "<=")
                return value <= target;
            else if (CompareOperator == "=")
                return value == target;
            else if (CompareOperator == "!=")
                return value != target;

            Debug.LogWarning($"Unsupported operator: '{CompareOperator}'.");
            return false;
        }

        /// <summary>
        /// 将浮点数值与目标进行比较
        /// </summary>
        /// <param name="value">待比较的浮点数值</param>
        /// <returns>满足比较条件时返回 true</returns>
        public bool CompareTo(double value)
        {
            if (TryGetSingleValue(out double target) == false)
                return false;

            if (CompareOperator == ">")
                return value > target;
            else if (CompareOperator == ">=")
                return value >= target;
            else if (CompareOperator == "<")
                return value < target;
            else if (CompareOperator == "<=")
                return value <= target;
            else if (CompareOperator == "=")
                return Math.Abs(value - target) < 1e-6;
            else if (CompareOperator == "!=")
                return Math.Abs(value - target) >= 1e-6;

            Debug.LogWarning($"Unsupported operator: '{CompareOperator}'.");
            return false;
        }
    }
}