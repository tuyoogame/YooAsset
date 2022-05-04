using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal abstract class BundledProvider : ProviderBase
    {
		protected AssetBundleLoaderBase OwnerBundle { private set; get; }
		protected DependAssetBundleGroup DependBundles { private set; get; }

		public BundledProvider(string assetPath, System.Type assetType) : base(assetPath, assetType)
		{
			OwnerBundle = AssetSystem.CreateOwnerAssetBundleLoader(assetPath);
			OwnerBundle.Reference();
			OwnerBundle.AddProvider(this);
			DependBundles = new DependAssetBundleGroup(assetPath);
			DependBundles.Reference();
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
			if (DependBundles != null)
			{
				DependBundles.Release();
				DependBundles = null;
			}
		}

		/// <summary>
		/// 获取资源包的调试信息列表
		/// </summary>
		internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
		{
			var bundleInfo = new DebugBundleInfo();
			bundleInfo.BundleName = OwnerBundle.BundleFileInfo.BundleName;
			bundleInfo.RefCount = OwnerBundle.RefCount;
			bundleInfo.Status = OwnerBundle.Status;
			output.Add(bundleInfo);

			DependBundles.GetBundleDebugInfos(output);
		}
	}
}