using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YooAsset.Editor
{
	/// <summary>
	/// 构建报告
	/// </summary>
	[Serializable]
	public class BuildReport
	{
		/// <summary>
		/// 构建汇总信息
		/// </summary>
		public BuildSummary Summary = new BuildSummary();

		/// <summary>
		/// 资源对象列表
		/// </summary>
		public List<ReportAssetInfo> AssetInfos = new List<ReportAssetInfo>();

		/// <summary>
		/// 资源包列表
		/// </summary>
		public List<ReportBundleInfo> BundleInfos = new List<ReportBundleInfo>();


		/// <summary>
		/// 序列化
		/// </summary>
		public static void Serialize(string savePath, BuildReport buildReport)
		{
			string json = JsonUtility.ToJson(buildReport, true);
			FileUtility.CreateFile(savePath, json);
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		public static BuildReport Deserialize(string jsonData)
		{
			BuildReport report = JsonUtility.FromJson<BuildReport>(jsonData);
			return report;
		}
	}
}