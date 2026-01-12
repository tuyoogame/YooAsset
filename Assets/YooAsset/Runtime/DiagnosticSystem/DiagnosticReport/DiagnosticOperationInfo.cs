using System;
using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 描述异步操作的运行时诊断信息
    /// </summary>
    [Serializable]
    internal class DiagnosticOperationInfo : IComparable<DiagnosticOperationInfo>
    {
        /// <summary>
        /// 异步操作的名称
        /// </summary>
        public string OperationName;

        /// <summary>
        /// 异步操作的说明
        /// </summary>
        public string OperationDescription;

        /// <summary>
        /// 异步操作的优先级
        /// </summary>
        public uint Priority;

        /// <summary>
        /// 开始的时间
        /// </summary>
        public string StartTime;

        /// <summary>
        /// 处理耗时（单位：毫秒）
        /// </summary>
        public long ElapsedMilliseconds;

        /// <summary>
        /// 异步操作的执行进度
        /// </summary>
        public float Progress;

        /// <summary>
        /// 异步操作的执行状态
        /// </summary>
        public string Status;

        /// <summary>
        /// 子任务列表
        /// </summary>
        /// <remarks>
        /// TODO：序列化深度限制为10层
        /// </remarks>
        public List<DiagnosticOperationInfo> Children;

        /// <inheritdoc />
        public int CompareTo(DiagnosticOperationInfo other)
        {
            return string.CompareOrdinal(OperationName, other.OperationName);
        }
    }
}
