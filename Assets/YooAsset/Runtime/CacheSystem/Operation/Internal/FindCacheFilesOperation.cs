using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal sealed class FindCacheFilesOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			Prepare,
			UpdateCacheFiles,
			Done,
		}

		private readonly PersistentManager _persistent;
		private readonly CacheManager _cache;
		private IEnumerator<DirectoryInfo> _filesEnumerator = null;
		private float _verifyStartTime;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 需要验证的元素
		/// </summary>
		public readonly List<VerifyCacheFileElement> VerifyElements = new List<VerifyCacheFileElement>(5000);

		public FindCacheFilesOperation(PersistentManager persistent, CacheManager cache)
		{
			_persistent = persistent;
			_cache = cache;
		}
		internal override void InternalOnStart()
		{
			_steps = ESteps.Prepare;
			_verifyStartTime = UnityEngine.Time.realtimeSinceStartup;
		}
		internal override void InternalOnUpdate()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.Prepare)
			{
				string rootPath = _persistent.SandboxCacheFilesRoot;
				DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
				if (rootDirectory.Exists)
				{
					var directorieInfos = rootDirectory.EnumerateDirectories();
					_filesEnumerator = directorieInfos.GetEnumerator();
				}
				_steps = ESteps.UpdateCacheFiles;
			}

			if (_steps == ESteps.UpdateCacheFiles)
			{
				if (UpdateCacheFiles())
					return;

				// 注意：总是返回成功
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
				float costTime = UnityEngine.Time.realtimeSinceStartup - _verifyStartTime;
				YooLogger.Log($"Find cache files elapsed time {costTime:f1} seconds");
			}
		}

		private bool UpdateCacheFiles()
		{
			if (_filesEnumerator == null)
				return false;

			bool isFindItem;
			while (true)
			{
				isFindItem = _filesEnumerator.MoveNext();
				if (isFindItem == false)
					break;

				var rootFoder = _filesEnumerator.Current;
				var childDirectories = rootFoder.GetDirectories();
				foreach (var chidDirectory in childDirectories)
				{
					string cacheGUID = chidDirectory.Name;
					if (_cache.IsCached(cacheGUID))
						continue;

					// 创建验证元素类
					string fileRootPath = chidDirectory.FullName;
					string dataFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleDataFileName}";
					string infoFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleInfoFileName}";
					if (_persistent.AppendFileExtension)
					{
						string fileExtension = FindFileExtension(chidDirectory);
						if (string.IsNullOrEmpty(fileExtension) == false)
							dataFilePath += fileExtension;
					}
					VerifyCacheFileElement element = new VerifyCacheFileElement(_cache.PackageName, cacheGUID, fileRootPath, dataFilePath, infoFilePath);
					VerifyElements.Add(element);
				}

				if (OperationSystem.IsBusy)
					break;
			}

			return isFindItem;
		}
		private string FindFileExtension(DirectoryInfo directoryInfo)
		{
			string dataFileExtension = string.Empty;
			var fileInfos = directoryInfo.GetFiles();
			foreach (var fileInfo in fileInfos)
			{
				if (fileInfo.Extension == ".temp")
					continue;
				if (fileInfo.Name.StartsWith(YooAssetSettings.CacheBundleDataFileName))
				{
					dataFileExtension = fileInfo.Extension;
					break;
				}
			}
			return dataFileExtension;
		}
	}
}