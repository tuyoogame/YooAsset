using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace YooAsset
{
	/// <summary>
	/// 清理本地包裹未使用的缓存文件
	/// </summary>
	public sealed class ClearPackageUnusedCacheFilesOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			GetUnusedCacheFiles,
			ClearUnusedCacheFiles,
			Done,
		}

		private readonly AssetsPackage _package;
		private ESteps _steps = ESteps.None;
		private List<string> _unusedCacheFilePaths;
		private int _unusedFileTotalCount = 0;

		internal ClearPackageUnusedCacheFilesOperation(AssetsPackage package)
		{
			_package = package;
		}
		internal override void Start()
		{
			_steps = ESteps.GetUnusedCacheFiles;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.GetUnusedCacheFiles)
			{
				_unusedCacheFilePaths = GetUnusedCacheFilePaths();
				_unusedFileTotalCount = _unusedCacheFilePaths.Count;
				YooLogger.Log($"Found unused cache file count : {_unusedFileTotalCount}");
				_steps = ESteps.ClearUnusedCacheFiles;
			}

			if (_steps == ESteps.ClearUnusedCacheFiles)
			{
				for (int i = _unusedCacheFilePaths.Count - 1; i >= 0; i--)
				{
					string filePath = _unusedCacheFilePaths[i];
					if (File.Exists(filePath))
					{
						try
						{
							File.Delete(filePath);
							YooLogger.Log($"Delete unused cache file : {filePath}");
						}
						catch (System.Exception e)
						{
							YooLogger.Warning($"Failed delete cache file : {filePath} ! {e.Message}");
						}
					}
					_unusedCacheFilePaths.RemoveAt(i);

					if (OperationSystem.IsBusy)
						break;
				}

				if (_unusedFileTotalCount == 0)
					Progress = 1.0f;
				else
					Progress = 1.0f - (_unusedCacheFilePaths.Count / _unusedFileTotalCount);

				if (_unusedCacheFilePaths.Count == 0)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
			}
		}

		/// <summary>
		/// 获取未被使用的缓存文件路径集合
		/// </summary>
		private List<string> GetUnusedCacheFilePaths()
		{
			string cacheFolderPath = PersistentHelper.GetCacheFolderPath(_package.PackageName);
			if (Directory.Exists(cacheFolderPath) == false)
				return new List<string>();

			DirectoryInfo directoryInfo = new DirectoryInfo(cacheFolderPath);
			FileInfo[] fileInfos = directoryInfo.GetFiles();
			List<string> result = new List<string>(fileInfos.Length);
			foreach (FileInfo fileInfo in fileInfos)
			{
				if (_package.IsIncludeBundleFile(fileInfo.Name) == false)
				{
					result.Add(fileInfo.FullName);
				}
			}
			return result;
		}
	}
}