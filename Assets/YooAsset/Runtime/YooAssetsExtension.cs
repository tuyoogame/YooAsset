using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	public static partial class YooAssets
	{
		private static YooAssetPackage _mainPackage;

		/// <summary>
		/// 是否已经初始化
		/// </summary>
		public static bool IsInitialized
		{
			get 
			{
				if (_mainPackage == null)
					return false;
				return _mainPackage.IsInitialized;
			}
		}

		/// <summary>
		/// 异步初始化
		/// </summary>
		public static InitializationOperation InitializeAsync(InitializeParameters parameters, string packageName = "DefaultPackage")
		{
			if (_mainPackage != null)
				throw new Exception("Main package is initialized yet.");

			_mainPackage = CreateAssetPackage(packageName);
			return _mainPackage.InitializeAsync(parameters);
		}

		/// <summary>
		/// 向网络端请求静态资源版本
		/// </summary>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public static UpdateStaticVersionOperation UpdateStaticVersionAsync(int timeout = 60)
		{
			return _mainPackage.UpdateStaticVersionAsync(timeout);
		}

		/// <summary>
		/// 向网络端请求并更新补丁清单
		/// </summary>
		/// <param name="packageCRC">更新的资源包裹版本</param>
		/// <param name="timeout">超时时间（默认值：60秒）</param>
		public static UpdateManifestOperation UpdateManifestAsync(string packageCRC, int timeout = 60)
		{
			return _mainPackage.UpdateManifestAsync(packageCRC, timeout);
		}

		/// <summary>
		/// 弱联网情况下加载补丁清单
		/// 注意：当指定版本内容验证失败后会返回失败。
		/// </summary>
		/// <param name="packageCRC">指定的资源包裹版本</param>
		public static UpdateManifestOperation WeaklyUpdateManifestAsync(string packageCRC)
		{
			return _mainPackage.WeaklyUpdateManifestAsync(packageCRC);
		}

		/// <summary>
		/// 资源回收（卸载引用计数为零的资源）
		/// </summary>
		public static void UnloadUnusedAssets()
		{
			_mainPackage.UnloadUnusedAssets();
		}

		/// <summary>
		/// 强制回收所有资源
		/// </summary>
		public static void ForceUnloadAllAssets()
		{
			_mainPackage.ForceUnloadAllAssets();
		}


		#region 资源信息
		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static bool IsNeedDownloadFromRemote(string location)
		{
			return _mainPackage.IsNeedDownloadFromRemote(location);
		}

		/// <summary>
		/// 是否需要从远端更新下载
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
		{
			return _mainPackage.IsNeedDownloadFromRemote(assetInfo);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tag">资源标签</param>
		public static AssetInfo[] GetAssetInfos(string tag)
		{
			return _mainPackage.GetAssetInfos(tag);
		}

		/// <summary>
		/// 获取资源信息列表
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		public static AssetInfo[] GetAssetInfos(string[] tags)
		{
			return _mainPackage.GetAssetInfos(tags);
		}

		/// <summary>
		/// 获取资源信息
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		public static AssetInfo GetAssetInfo(string location)
		{
			return _mainPackage.GetAssetInfo(location);
		}

		/// <summary>
		/// 获取资源路径
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <returns>如果location地址无效，则返回空字符串</returns>
		public static string GetAssetPath(string location)
		{
			return _mainPackage.GetAssetPath(location);
		}
		#endregion

		#region 原生文件
		/// <summary>
		/// 异步获取原生文件
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="copyPath">拷贝路径</param>
		public static RawFileOperation GetRawFileAsync(string location, string copyPath = null)
		{
			return _mainPackage.GetRawFileAsync(location, copyPath);
		}

		/// <summary>
		/// 异步获取原生文件
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		/// <param name="copyPath">拷贝路径</param>
		public static RawFileOperation GetRawFileAsync(AssetInfo assetInfo, string copyPath = null)
		{
			return _mainPackage.GetRawFileAsync(assetInfo, copyPath);
		}
		#endregion

		#region 场景加载
		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="location">场景的定位地址</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(string location, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			return _mainPackage.LoadSceneAsync(location, sceneMode, activateOnLoad, priority);
		}

		/// <summary>
		/// 异步加载场景
		/// </summary>
		/// <param name="assetInfo">场景的资源信息</param>
		/// <param name="sceneMode">场景加载模式</param>
		/// <param name="activateOnLoad">加载完毕时是否主动激活</param>
		/// <param name="priority">优先级</param>
		public static SceneOperationHandle LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
		{
			return _mainPackage.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad, priority);
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetSync(AssetInfo assetInfo)
		{
			return _mainPackage.LoadAssetSync(assetInfo);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			return _mainPackage.LoadAssetSync<TObject>(location);
		}

		/// <summary>
		/// 同步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public static AssetOperationHandle LoadAssetSync(string location, System.Type type)
		{
			return _mainPackage.LoadAssetSync(location, type);
		}


		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static AssetOperationHandle LoadAssetAsync(AssetInfo assetInfo)
		{
			return _mainPackage.LoadAssetAsync(assetInfo);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static AssetOperationHandle LoadAssetAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			return _mainPackage.LoadAssetAsync<TObject>(location);
		}

		/// <summary>
		/// 异步加载资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">资源类型</param>
		public static AssetOperationHandle LoadAssetAsync(string location, System.Type type)
		{
			return _mainPackage.LoadAssetAsync(location, type);
		}
		#endregion

		#region 资源加载
		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync(AssetInfo assetInfo)
		{
			return _mainPackage.LoadSubAssetsSync(assetInfo);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync<TObject>(string location) where TObject : UnityEngine.Object
		{
			return _mainPackage.LoadSubAssetsSync<TObject>(location);
		}

		/// <summary>
		/// 同步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public static SubAssetsOperationHandle LoadSubAssetsSync(string location, System.Type type)
		{
			return _mainPackage.LoadSubAssetsSync(location, type);
		}


		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="assetInfo">资源信息</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(AssetInfo assetInfo)
		{
			return _mainPackage.LoadSubAssetsAsync(assetInfo);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <typeparam name="TObject">资源类型</typeparam>
		/// <param name="location">资源的定位地址</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync<TObject>(string location) where TObject : UnityEngine.Object
		{
			return _mainPackage.LoadSubAssetsAsync<TObject>(location);
		}

		/// <summary>
		/// 异步加载子资源对象
		/// </summary>
		/// <param name="location">资源的定位地址</param>
		/// <param name="type">子对象类型</param>
		public static SubAssetsOperationHandle LoadSubAssetsAsync(string location, System.Type type)
		{
			return _mainPackage.LoadSubAssetsAsync(location, type);
		}
		#endregion

		#region 资源下载
		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(string tag, int downloadingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreatePatchDownloader(new string[] { tag }, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新资源标签指定的资源包文件
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreatePatchDownloader(tags, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新当前资源版本所有的资源包文件
		/// </summary>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreatePatchDownloader(int downloadingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreatePatchDownloader(downloadingMaxNumber, failedTryAgain);
		}


		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="locations">资源定位列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreateBundleDownloader(locations, downloadingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁下载器，用于下载更新指定的资源列表依赖的资源包文件
		/// </summary>
		/// <param name="assetInfos">资源信息列表</param>
		/// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
		/// <param name="failedTryAgain">下载失败的重试次数</param>
		public static PatchDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreateBundleDownloader(assetInfos, downloadingMaxNumber, failedTryAgain);
		}
		#endregion

		#region 资源解压
		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tag">资源标签</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreatePatchUnpacker(tag, unpackingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="tags">资源标签列表</param>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreatePatchUnpacker(tags, unpackingMaxNumber, failedTryAgain);
		}

		/// <summary>
		/// 创建补丁解压器
		/// </summary>
		/// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
		/// <param name="failedTryAgain">解压失败的重试次数</param>
		public static PatchUnpackerOperation CreatePatchUnpacker(int unpackingMaxNumber, int failedTryAgain)
		{
			return _mainPackage.CreatePatchUnpacker(unpackingMaxNumber, failedTryAgain);
		}
		#endregion

		#region 包裹更新
		/// <summary>
		/// 创建资源包裹下载器，用于下载更新指定资源版本所有的资源包文件
		/// </summary>
		/// <param name="packageCRC">指定更新的资源包裹版本</param>
		/// <param name="timeout">超时时间</param>
		public static UpdatePackageOperation UpdatePackageAsync(string packageCRC, int timeout = 60)
		{
			return _mainPackage.UpdatePackageAsync(packageCRC, timeout);
		}
		#endregion
	}
}