using System;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 远程玩家调试会话，管理单个玩家连接的诊断报告队列
    /// </summary>
    internal class RemotePlayerSession
    {
        private readonly List<DiagnosticReport> _reports = new List<DiagnosticReport>();

        /// <summary>
        /// 玩家 ID
        /// </summary>
        public int PlayerID { get; private set; }

        /// <summary>
        /// 报告缓存的最大容量
        /// </summary>
        public int MaxReportCount { get; private set; }

        /// <summary>
        /// 报告索引的最小值
        /// </summary>
        public int MinRangeValue
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 报告索引的最大值
        /// </summary>
        public int MaxRangeValue
        {
            get
            {
                return _reports.Count - 1;
            }
        }


        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="playerID">玩家的唯一标识</param>
        public RemotePlayerSession(int playerID)
        {
            PlayerID = playerID;
            MaxReportCount = 500;
        }

        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="playerID">玩家的唯一标识</param>
        /// <param name="maxReportCount">报告缓存的最大容量</param>
        public RemotePlayerSession(int playerID, int maxReportCount)
        {
            PlayerID = playerID;
            MaxReportCount = maxReportCount;
        }

        /// <summary>
        /// 清理所有缓存的诊断报告
        /// </summary>
        public void ClearDebugReport()
        {
            _reports.Clear();
        }

        /// <summary>
        /// 添加一份诊断报告
        /// </summary>
        /// <param name="report">待添加的诊断报告</param>
        public void AddDebugReport(DiagnosticReport report)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (_reports.Count >= MaxReportCount)
                _reports.RemoveAt(0);
            _reports.Add(report);
        }

        /// <summary>
        /// 获取指定索引处的诊断报告
        /// </summary>
        /// <param name="rangeIndex">报告索引</param>
        /// <returns>对应的诊断报告</returns>
        public DiagnosticReport GetDebugReport(int rangeIndex)
        {
            if (_reports.Count == 0)
                return null;
            if (rangeIndex < 0 || rangeIndex >= _reports.Count)
                return null;
            return _reports[rangeIndex];
        }

        /// <summary>
        /// 将索引值约束到有效范围内
        /// </summary>
        /// <param name="rangeIndex">待约束的索引值</param>
        /// <returns>约束后的有效索引</returns>
        public int ClampRangeIndex(int rangeIndex)
        {
            if (rangeIndex < 0)
                return 0;

            if (rangeIndex > MaxRangeValue)
                return MaxRangeValue;

            return rangeIndex;
        }
    }
}
