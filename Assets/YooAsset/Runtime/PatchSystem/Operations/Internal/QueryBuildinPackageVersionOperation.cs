
namespace YooAsset
{
	/// <summary>
	/// 内置补丁清单版本查询器
	/// </summary>
	internal class QueryBuildinPackageVersionOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadPackageVersion,
			CheckLoadPackageVersion,
			Done,
		}

		private readonly string _packageName;
		private UnityWebDataRequester _downloader;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 内置包裹版本
		/// </summary>
		public string Version { private set; get; }


		public QueryBuildinPackageVersionOperation(string packageName)
		{
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadPackageVersion;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadPackageVersion)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestVersionFileName(_packageName);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(url);
				_steps = ESteps.CheckLoadPackageVersion;
			}

			if (_steps == ESteps.CheckLoadPackageVersion)
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
					Version = _downloader.GetText();
					if (string.IsNullOrEmpty(Version))
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"Buildin package version file content is empty !";
					}
					else
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}

				_downloader.Dispose();
			}
		}
	}
}