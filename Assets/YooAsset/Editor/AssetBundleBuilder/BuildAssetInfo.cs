using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
	/// <summary>
	/// 构建的资源信息类
	/// </summary>
	public class BuildAssetInfo
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		public string AssetPath { private set; get; }

		/// <summary>
		/// 资源包标签
		/// </summary>
		public string BundleLabel { private set; get; }

		/// <summary>
		/// 资源包文件格式
		/// </summary>
		public string BundleVariant { private set; get; }

		/// <summary>
		/// 是否为原生资源
		/// </summary>
		public bool IsRawAsset = false;

		/// <summary>
		/// 是否为主动收集资源
		/// </summary>
		public bool IsCollectAsset = false;

		/// <summary>
		/// 资源标记列表
		/// </summary>
		public List<string> AssetTags = new List<string>();

		/// <summary>
		/// 被依赖次数
		/// </summary>
		public int DependCount = 0;

		/// <summary>
		/// 依赖的所有资源信息
		/// 注意：包括零依赖资源（零依赖资源的资源包名无效）
		/// </summary>
		public List<BuildAssetInfo> AllDependAssetInfos { private set; get; } = null;


		public BuildAssetInfo(string assetPath)
		{
			AssetPath = assetPath;
		}

		/// <summary>
		/// 设置所有依赖的资源
		/// </summary>
		public void SetAllDependAssetInfos(List<BuildAssetInfo> dependAssetInfos)
		{
			if (AllDependAssetInfos != null)
				throw new System.Exception("Should never get here !");

			AllDependAssetInfos = dependAssetInfos;
		}

		/// <summary>
		/// 设置资源包的标签和文件格式
		/// </summary>
		public void SetBundleLabelAndVariant(string bundleLabel, string bundleVariant)
		{
			if (string.IsNullOrEmpty(BundleLabel) == false || string.IsNullOrEmpty(BundleVariant) == false)
				throw new System.Exception("Should never get here !");

			BundleLabel = bundleLabel;
			BundleVariant = bundleVariant;
		}

		/// <summary>
		/// 添加资源标记
		/// </summary>
		public void AddAssetTags(List<string> tags)
		{
			foreach (var tag in tags)
			{
				if (AssetTags.Contains(tag) == false)
				{
					AssetTags.Add(tag);
				}
			}
		}

		/// <summary>
		/// 获取资源包的完整名称
		/// </summary>
		public string GetBundleName()
		{
			if (string.IsNullOrEmpty(BundleLabel) || string.IsNullOrEmpty(BundleVariant))
				throw new System.ArgumentNullException();

			return AssetBundleBuilderHelper.MakeBundleName(BundleLabel, BundleVariant);
		}

		/// <summary>
		/// 检测资源包名是否有效
		/// </summary>
		public bool CheckBundleNameValid()
		{
			if (string.IsNullOrEmpty(BundleLabel) == false && string.IsNullOrEmpty(BundleVariant) == false)
				return true;
			else
				return false;
		}
	}
}