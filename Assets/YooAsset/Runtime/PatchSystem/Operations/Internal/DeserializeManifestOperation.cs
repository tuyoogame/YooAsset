using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset
{
	internal class DeserializeManifestOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			DeserializeFileHeader,
			PrepareAssetList,
			DeserializeAssetList,
			PrepareBundleList,
			DeserializeBundleList,
			Done,
		}
	
		private readonly BufferReader _buffer;
		private int _patchAssetCount;
		private int _patchBundleCount;
		private int _progressTotalValue;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 解析的清单实例
		/// </summary>
		public PatchManifest Manifest { private set; get; }

		public DeserializeManifestOperation(byte[] binaryData)
		{
			_buffer = new BufferReader(binaryData);
		}
		internal override void Start()
		{
			_steps = ESteps.DeserializeFileHeader;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			try
			{
				if (_steps == ESteps.DeserializeFileHeader)
				{
					if (_buffer.IsValid == false)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = "Buffer is invalid !";
						return;
					}

					// 读取文件标记
					uint fileSign = _buffer.ReadUInt32();
					if (fileSign != YooAssetSettings.PatchManifestFileSign)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = "The manifest file format is invalid !";
						return;
					}

					// 读取文件版本
					string fileVersion = _buffer.ReadUTF8();
					if (fileVersion != YooAssetSettings.PatchManifestFileVersion)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Failed;
						Error = $"The manifest file version are not compatible : {fileVersion} != {YooAssetSettings.PatchManifestFileVersion}";
						return;
					}

					// 读取文件头信息
					Manifest = new PatchManifest();
					Manifest.FileVersion = fileVersion;
					Manifest.EnableAddressable = _buffer.ReadBool();
					Manifest.OutputNameStyle = _buffer.ReadInt32();
					Manifest.PackageName = _buffer.ReadUTF8();
					Manifest.PackageVersion = _buffer.ReadUTF8();

					_steps = ESteps.PrepareAssetList;
				}

				if (_steps == ESteps.PrepareAssetList)
				{
					_patchAssetCount = _buffer.ReadInt32();
					Manifest.AssetList = new List<PatchAsset>(_patchAssetCount);
					Manifest.AssetDic = new Dictionary<string, PatchAsset>(_patchAssetCount);
					_progressTotalValue = _patchAssetCount;
					_steps = ESteps.DeserializeAssetList;
				}
				if (_steps == ESteps.DeserializeAssetList)
				{
					while (_patchAssetCount > 0)
					{
						var patchAsset = new PatchAsset();
						patchAsset.Address = _buffer.ReadUTF8();
						patchAsset.AssetPath = _buffer.ReadUTF8();
						patchAsset.AssetTags = _buffer.ReadUTF8Array();
						patchAsset.BundleID = _buffer.ReadInt32();
						patchAsset.DependIDs = _buffer.ReadInt32Array();
						Manifest.AssetList.Add(patchAsset);

						// 注意：我们不允许原始路径存在重名
						string assetPath = patchAsset.AssetPath;
						if (Manifest.AssetDic.ContainsKey(assetPath))
							throw new System.Exception($"AssetPath have existed : {assetPath}");
						else
							Manifest.AssetDic.Add(assetPath, patchAsset);

						_patchAssetCount--;
						Progress = 1f - _patchAssetCount / _progressTotalValue;
						if (OperationSystem.IsBusy)
							break;
					}

					if (_patchAssetCount <= 0)
					{
						_steps = ESteps.PrepareBundleList;
					}
				}

				if (_steps == ESteps.PrepareBundleList)
				{
					_patchBundleCount = _buffer.ReadInt32();
					Manifest.BundleList = new List<PatchBundle>(_patchBundleCount);
					Manifest.BundleDic = new Dictionary<string, PatchBundle>(_patchBundleCount);
					_progressTotalValue = _patchBundleCount;
					_steps = ESteps.DeserializeBundleList;
				}
				if (_steps == ESteps.DeserializeBundleList)
				{
					while (_patchBundleCount > 0)
					{
						var patchBundle = new PatchBundle();
						patchBundle.BundleName = _buffer.ReadUTF8();
						patchBundle.FileHash = _buffer.ReadUTF8();
						patchBundle.FileCRC = _buffer.ReadUTF8();
						patchBundle.FileSize = _buffer.ReadInt64();
						patchBundle.IsRawFile = _buffer.ReadBool();
						patchBundle.LoadMethod = _buffer.ReadByte();
						patchBundle.Tags = _buffer.ReadUTF8Array();
						Manifest.BundleList.Add(patchBundle);

						patchBundle.ParseBundle(Manifest.PackageName, Manifest.OutputNameStyle);
						Manifest.BundleDic.Add(patchBundle.BundleName, patchBundle);

						_patchBundleCount--;
						Progress = 1f - _patchBundleCount / _progressTotalValue;
						if (OperationSystem.IsBusy)
							break;
					}

					if (_patchBundleCount <= 0)
					{
						_steps = ESteps.Done;
						Status = EOperationStatus.Succeed;
					}
				}
			}
			catch (System.Exception e)
			{
				Manifest = null;
				_steps = ESteps.Done;
				Status = EOperationStatus.Failed;
				Error = e.Message;
			}
		}
	}
}