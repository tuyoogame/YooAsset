using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
    internal abstract class BundledProvider : AssetProviderBase
    {
		protected BundleFileLoader OwnerBundle { private set; get; }
		protected DependBundleGrouper DependBundles { private set; get; }

		public BundledProvider(string assetPath, System.Type assetType) : base(assetPath, assetType)
		{
			OwnerBundle = AssetSystem.CreateOwnerBundleLoader(assetPath);
			OwnerBundle.Reference();
			OwnerBundle.AddProvider(this);
			DependBundles = new DependBundleGrouper(assetPath);
			DependBundles.Reference();
		}
		public override void Destory()
		{
			base.Destory();

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
		internal void GetBundleDebugInfos(List<DebugSummy.BundleInfo> output)
		{
			var ownerInfo = new DebugSummy.BundleInfo();
			ownerInfo.BundleName = OwnerBundle.BundleInfo.BundleName;
			ownerInfo.Version = OwnerBundle.BundleInfo.Version;
			ownerInfo.RefCount = OwnerBundle.RefCount;
			ownerInfo.States = OwnerBundle.States;
			output.Add(ownerInfo);

			DependBundles.GetBundleDebugInfos(output);
		}
	}
}