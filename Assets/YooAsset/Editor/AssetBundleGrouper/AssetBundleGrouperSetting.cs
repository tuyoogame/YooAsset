using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YooAsset.Editor
{
	public class AssetBundleGrouperSetting : ScriptableObject
	{
		/// <summary>
		/// 是否启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable = false;

		/// <summary>
		/// 自动收集着色器
		/// </summary>
		public bool AutoCollectShaders = true;

		/// <summary>
		/// 自动收集的着色器资源包名称
		/// </summary>
		public string ShadersBundleName = "myshaders";

		/// <summary>
		/// 分组列表
		/// </summary>
		public List<AssetBundleGrouper> Groupers = new List<AssetBundleGrouper>();


		/// <summary>
		/// 检测配置错误
		/// </summary>
		public void CheckConfigError()
		{
			foreach (var grouper in Groupers)
			{
				grouper.CheckConfigError();
			}
		}

		/// <summary>
		/// 获取打包收集的资源文件
		/// </summary>
		public List<CollectAssetInfo> GetAllCollectAssets()
		{
			Dictionary<string, CollectAssetInfo> result = new Dictionary<string, CollectAssetInfo>(10000);

			// 收集打包资源
			foreach (var grouper in Groupers)
			{
				var temper = grouper.GetAllCollectAssets();
				foreach (var assetInfo in temper)
				{
					if (result.ContainsKey(assetInfo.AssetPath) == false)
						result.Add(assetInfo.AssetPath, assetInfo);
					else
						throw new Exception($"The collecting asset file is existed : {assetInfo.AssetPath} in grouper setting.");
				}
			}

			// 检测可寻址地址是否重复
			if (EnableAddressable)
			{
				HashSet<string> adressTemper = new HashSet<string>();
				foreach (var collectInfoPair in result)
				{
					if (collectInfoPair.Value.CollectorType == ECollectorType.MainCollector)
					{
						string address = collectInfoPair.Value.Address;
						if (adressTemper.Contains(address) == false)
							adressTemper.Add(address);
						else
							throw new Exception($"The address is existed : {address} in grouper setting.");
					}
				}
			}

			// 返回列表
			return result.Values.ToList();
		}
	}
}