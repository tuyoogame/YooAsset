using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal abstract class BundledProvider : ProviderBase
	{
		protected BundleLoaderBase OwnerBundle { private set; get; }
		protected DependAssetBundleGroup DependBundleGroup { private set; get; }

		public BundledProvider(AssetSystemImpl impl, string providerGUID, AssetInfo assetInfo) : base(impl, providerGUID, assetInfo)
		{
			OwnerBundle = impl.CreateOwnerAssetBundleLoader(assetInfo);
			OwnerBundle.Reference();
			OwnerBundle.AddProvider(this);

			var dependBundles = impl.CreateDependAssetBundleLoaders(assetInfo);
			DependBundleGroup = new DependAssetBundleGroup(dependBundles);
			DependBundleGroup.Reference();
		}
		public override void Destroy()
		{
			base.Destroy();

			// 释放资源包
			if (OwnerBundle != null)
			{
				OwnerBundle.Release();
				OwnerBundle = null;
			}
			if (DependBundleGroup != null)
			{
				DependBundleGroup.Release();
				DependBundleGroup = null;
			}
		}

		/// <summary>
		/// 获取资源包的调试信息列表
		/// </summary>
		internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
		{
			var bundleInfo = new DebugBundleInfo();
			bundleInfo.BundleName = OwnerBundle.MainBundleInfo.Bundle.BundleName;
			bundleInfo.RefCount = OwnerBundle.RefCount;
			bundleInfo.Status = OwnerBundle.Status.ToString();
			output.Add(bundleInfo);

			DependBundleGroup.GetBundleDebugInfos(output);
		}
	}
}