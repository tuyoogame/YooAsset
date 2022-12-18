using System.IO;

namespace YooAsset
{
	/// <summary>
	/// 沙盒补丁清单加载器
	/// </summary>
	internal class CacheManifestLoadOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			LoadCacheManifestFile,
			CheckDeserializeManifest,
			Done,
		}

		private readonly string _packageName;
		private DeserializeManifestOperation _deserializer;
		private ESteps _steps = ESteps.None;
		private string _manifestFilePath;

		/// <summary>
		/// 加载结果
		/// </summary>
		public PatchManifest Manifest { private set; get; }


		public CacheManifestLoadOperation(string packageName)
		{
			_packageName = packageName;
		}
		internal override void Start()
		{
			_steps = ESteps.LoadCacheManifestFile;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.LoadCacheManifestFile)
			{
				_manifestFilePath = PersistentHelper.GetCacheManifestFilePath(_packageName);
				if (File.Exists(_manifestFilePath) == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"Manifest file not found : {_manifestFilePath}";
					return;
				}

				byte[] bytesData = File.ReadAllBytes(_manifestFilePath);
				_deserializer = new DeserializeManifestOperation(bytesData);
				OperationSystem.StartOperation(_deserializer);
				_steps = ESteps.CheckDeserializeManifest;
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

					// 注意：如果加载沙盒内的清单报错，为了避免流程被卡住，我们主动把损坏的文件删除。
					if (File.Exists(_manifestFilePath))
					{
						YooLogger.Warning($"Failed to load cache manifest file : {Error}");
						YooLogger.Warning($"Invalid cache manifest file have been removed : {_manifestFilePath}");
						File.Delete(_manifestFilePath);
					}
				}
			}
		}
	}
}