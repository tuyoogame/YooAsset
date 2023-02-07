using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class PackageCachingOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			GetCacheFiles,
			VerifyCacheFiles,
			Done,
		}

		private readonly string _packageName;
		private PackageVerifyOperation _packageVerifyOp;
		private ESteps _steps = ESteps.None;

		public PackageCachingOperation(string packageName)
		{
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.GetCacheFiles;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.GetCacheFiles)
			{
				var elements = GetVerifyElements();
				_packageVerifyOp = PackageVerifyOperation.CreateOperation(elements);
				OperationSystem.StartOperation(_packageVerifyOp);
				_steps = ESteps.VerifyCacheFiles;
			}

			if (_steps == ESteps.VerifyCacheFiles)
			{
				Progress = _packageVerifyOp.Progress;
				if (_packageVerifyOp.IsDone == false)
					return;

				// 注意：总是返回成功
				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}

		private List<VerifyElement> GetVerifyElements()
		{
			string cacheFolderPath = PersistentHelper.GetCacheFolderPath(_packageName);
			if (Directory.Exists(cacheFolderPath) == false)
				return new List<VerifyElement>();

			DirectoryInfo rootDirectory = new DirectoryInfo(cacheFolderPath);
			DirectoryInfo[] fileFolders = rootDirectory.GetDirectories();
			List<VerifyElement> result = new List<VerifyElement>(fileFolders.Length);
			foreach (var fileFoder in fileFolders)
			{
				string cacheGUID = fileFoder.Name;
				if (CacheSystem.IsCached(_packageName, cacheGUID))
					continue;

				string fileRootPath = fileFoder.FullName;
				string dataFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleDataFileName}";
				string infoFilePath = $"{fileRootPath}/{ YooAssetSettings.CacheBundleInfoFileName}";
				VerifyElement element = new VerifyElement(_packageName, cacheGUID, fileRootPath, dataFilePath, infoFilePath);
				result.Add(element);
			}

			return result;
		}
	}
}