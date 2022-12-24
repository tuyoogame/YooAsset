
namespace YooAsset
{
	internal class CopyBuildinPackageHashOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			CopyBuildinPackageHashFile,
			Done,
		}

		private readonly string _buildinPackageName;
		private readonly string _buildinPackageVersion;
		private UnityWebFileRequester _downloader;
		private ESteps _steps = ESteps.None;

		public CopyBuildinPackageHashOperation(string buildinPackageName, string buildinPackageVersion)
		{
			_buildinPackageName = buildinPackageName;
			_buildinPackageVersion = buildinPackageVersion;
		}
		internal override void Start()
		{
			_steps = ESteps.CopyBuildinPackageHashFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.CopyBuildinPackageHashFile)
			{
				if (_downloader == null)
				{
					string savePath = PersistentHelper.GetCachePackageHashFilePath(_buildinPackageName, _buildinPackageVersion);
					string fileName = YooAssetSettingsData.GetPackageHashFileName(_buildinPackageName, _buildinPackageVersion);
					string filePath = PathHelper.MakeStreamingLoadPath(fileName);
					string url = PathHelper.ConvertToWWWPath(filePath);
					_downloader = new UnityWebFileRequester();
					_downloader.SendRequest(url, savePath);
				}

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