using System;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset.Editor
{
	internal class RemotePlayerSession
	{
		private readonly List<DebugReport> _reportList = new List<DebugReport>();

		/// <summary>
		/// 用户ID
		/// </summary>
		public int PlayerId { private set; get; }

		/// <summary>
		/// 保存的报告最大数量
		/// </summary>
		public int MaxReportCount { private set; get; }


		public RemotePlayerSession(int playerId, int maxReportCount = 1000)
		{
			PlayerId = playerId;
			MaxReportCount = maxReportCount;
		}

		/// <summary>
		/// 添加一个调试报告
		/// </summary>
		public void AddDebugReport(DebugReport report)
		{
			if (report == null)
				Debug.LogWarning("Invalid debug report data !");

			if (_reportList.Count >= MaxReportCount)
				_reportList.RemoveAt(0);
			_reportList.Add(report);
		}

		/// <summary>
		/// 获取最近一次的报告
		/// </summary>
		public DebugReport GetLatestReport()
		{
			if (_reportList.Count == 0)
				return null;
			return _reportList[_reportList.Count - 1];
		}
	}
}