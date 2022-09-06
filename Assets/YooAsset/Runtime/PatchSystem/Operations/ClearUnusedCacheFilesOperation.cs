using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 清理未使用的缓存资源操作类
	/// </summary>
	public abstract class ClearUnusedCacheFilesOperation : AsyncOperationBase
	{
	}

	/// <summary>
	/// 编辑器模式
	/// </summary>
	internal sealed class EditorPlayModeClearUnusedCacheFilesOperation : ClearUnusedCacheFilesOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 离线模式
	/// </summary>
	internal sealed class OfflinePlayModeClearUnusedCacheFilesOperation : ClearUnusedCacheFilesOperation
	{
		internal override void Start()
		{
			Status = EOperationStatus.Succeed;
		}
		internal override void Update()
		{
		}
	}

	/// <summary>
	/// 联机模式
	/// </summary>
	internal sealed class HostPlayModeClearUnusedCacheFilesOperation : ClearUnusedCacheFilesOperation
	{
		private enum ESteps
		{
			None,
			GetUnusedCacheFiles,
			ClearUnusedCacheFiles,
			Done,
		}

		private ESteps _steps = ESteps.None;
		private List<string> _unusedCacheFilePaths;
		private int _unusedFileTotalCount = 0;
		private HostPlayModeImpl _impl;

		internal HostPlayModeClearUnusedCacheFilesOperation(HostPlayModeImpl impl)
		{
			_impl = impl;
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
				_unusedCacheFilePaths = _impl.ClearUnusedCacheFilePaths();
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
						YooLogger.Log($"Delete unused cache file : {filePath}");
						File.Delete(filePath);
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
	}
}