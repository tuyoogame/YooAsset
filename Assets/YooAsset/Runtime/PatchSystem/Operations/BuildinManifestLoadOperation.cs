
namespace YooAsset
{
	/// <summary>
	/// 内置补丁清单加载器
	/// </summary>
	internal class BuildinManifestLoadOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadBuildinManifest,
			CheckLoadBuildinManifest,
			CheckDeserializeManifest,
			Done,
		}

		private readonly string _buildinPackageName;
		private readonly string _buildinPackageVersion;
		private UnityWebDataRequester _downloader;
		private DeserializeManifestOperation _deserializer;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 加载结果
		/// </summary>
		public PatchManifest Manifest { private set; get; }


		public BuildinManifestLoadOperation(string buildinPackageName, string buildinPackageVersion)
		{
			_buildinPackageName = buildinPackageName;
			_buildinPackageVersion = buildinPackageVersion;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadBuildinManifest;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadBuildinManifest)
			{
				string fileName = YooAssetSettingsData.GetPatchManifestBinaryFileName(_buildinPackageName, _buildinPackageVersion);
				string filePath = PathHelper.MakeStreamingLoadPath(fileName);
				string url = PathHelper.ConvertToWWWPath(filePath);
				_downloader = new UnityWebDataRequester();
				_downloader.SendRequest(url);
				_steps = ESteps.CheckLoadBuildinManifest;
			}

			if (_steps == ESteps.CheckLoadBuildinManifest)
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
					byte[] bytesData = _downloader.GetData();
					_deserializer = new DeserializeManifestOperation(bytesData);
					OperationSystem.StartOperation(_deserializer);
					_steps = ESteps.CheckDeserializeManifest;
				}

				_downloader.Dispose();
			}

			if (_steps == ESteps.CheckDeserializeManifest)
			{
				Progress = _deserializer.Progress;
				if (_deserializer.IsDone == false)
					return;

				if (_deserializer.Status == EOperationStatus.Succeed)
				{
					Manifest = _deserializer.Manifest;
					_steps = ESteps.Done;
					Status = EOperationStatus.Succeed;
				}
				else
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = _deserializer.Error;
				}
			}
		}
	}
}