
namespace YooAsset
{
	/// <summary>
	/// 内置补丁清单复制器
	/// </summary>
	internal class BuildinManifestCopyOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			CopyBuildinManifest,
			CheckCopyBuildinManifest,
			Done,
		}

		private readonly string _buildinPackageName;
		private readonly string _buildinPackageVersion;
		private UnityWebFileRequester _downloader;
		private ESteps _steps = ESteps.None;


		public BuildinManifestCopyOperation(string buildinPackageName, string buildinPackageVersion)
		{
			_buildinPackageName = buildinPackageName;
			_buildinPackageVersion = buildinPackageVersion;
		}
		internal override void Start()
		{
			_steps = ESteps.CopyBuildinManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.CopyBuildinManifest)
			{
				string savePath = PersistentHelper.GetCacheManifestFilePath(_buildinPackageName);
				string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_buildinPackageName, _buildinPackageVersion);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebFileRequester();
				_downloader.SendRequest(url, savePath);
				_steps = ESteps.CheckCopyBuildinManifest;
			}

			if (_steps == ESteps.CheckCopyBuildinManifest)
			{
				if (_downloader.IsDone() == false)
					return;

				if (_downloader.HasError())
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _downloader.GetError();
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}

				_downloader.Dispose();
			}
		}
	}
}