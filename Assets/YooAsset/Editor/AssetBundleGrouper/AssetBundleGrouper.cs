using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	[Serializable]
	public class AssetBundleGrouper
	{
		/// <summary>
		/// 分组名称
		/// </summary>
		public string GrouperName = string.Empty;

		/// <summary>
		/// 分组描述
		/// </summary>
		public string GrouperDesc = string.Empty;

		/// <summary>
		/// 资源分类标签
		/// </summary>
		public string AssetTags = string.Empty;

		/// <summary>
		/// 分组的收集器列表
		/// </summary>
		public List<AssetBundleCollector> Collectors = new List<AssetBundleCollector>();


		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			foreach (var collector in Collectors)
			{
				collector.CheckConfigError();
			}
		}

		/// <summary>
		/// 获取打包收集的资源文件
		/// </summary>
		public List<CollectAssetInfo> GetAllCollectAssets()
		{
			Dictionary<string, string> adressTemper = new Dictionary<string, string>(10000);
			Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(10000);
			foreach (var collector in Collectors)
			{
				var temper = collector.GetAllCollectAssets(this);
				foreach (var assetInfo in temper)
				{
					if (result.ContainsKey(assetInfo.AssetPath) == false)
					{
						result.Add(assetInfo.AssetPath, assetInfo);
					}
					else
					{
						throw new Exception($"The collecting asset file is existed : {assetInfo.AssetPath} in grouper : {GrouperName}");
					}
				}
			}

			// 检测可寻址地址是否重复
			if (AssetBundleGrouperSettingData.Setting.EnableAddressable)
			{
				foreach (var collectInfo in result)
				{
					string address = collectInfo.Value.Address;
					if (adressTemper.ContainsKey(address) == false)
					{
						adressTemper.Add(address, address);
					}
					else
					{
						throw new Exception($"The address is existed : {address} in grouper : {GrouperName}");
					}
				}
			}

			return result.Values.ToList();
		}
	}
}