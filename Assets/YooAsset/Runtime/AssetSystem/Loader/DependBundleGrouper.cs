using System;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class DependBundleGrouper
	{
		/// <summary>
		/// 依赖的资源包加载器列表
		/// </summary>
		private readonly List<BundleFileLoader> _dependBundles;

		public DependBundleGrouper(string assetPath)
		{
			_dependBundles = AssetSystem.CreateDependBundleLoaders(assetPath);
		}

		/// <summary>
		/// 是否已经完成（无论成功或失败）
		/// </summary>
		public bool IsDone()
		{
			foreach (var loader in _dependBundles)
			{
				if (loader.IsDone() == false)
					return false;
			}
			return true;
		}

		/// <summary>
		/// 主线程等待异步操作完毕
		/// </summary>
		public void WaitForAsyncComplete()
		{
			foreach (var loader in _dependBundles)
			{
				if (loader.IsDone() == false)
					loader.WaitForAsyncComplete();
			}
		}

		/// <summary>
		/// 增加引用计数
		/// </summary>
		public void Reference()
		{
			foreach (var loader in _dependBundles)
			{
				loader.Reference();
			}
		}

		/// <summary>
		/// 减少引用计数
		/// </summary>
		public void Release()
		{
			foreach (var loader in _dependBundles)
			{
				loader.Release();
			}
		}

		/// <summary>
		/// 获取资源包的调试信息列表
		/// </summary>
		internal void GetBundleDebugInfos(List<DebugSummy.DebugBundleInfo> output)
		{
			foreach (var loader in _dependBundles)
			{
				var debugInfo = new DebugSummy.DebugBundleInfo();
				debugInfo.BundleName = loader.BundleFileInfo.BundleName;
				debugInfo.Version = loader.BundleFileInfo.Version;
				debugInfo.RefCount = loader.RefCount;
				debugInfo.States = loader.States;
				output.Add(debugInfo);
			}
		}
	}
}