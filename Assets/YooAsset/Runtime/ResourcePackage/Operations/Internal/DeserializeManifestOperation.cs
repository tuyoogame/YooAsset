using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

namespace YooAsset
{
    /// <summary>
    /// 反序列化清单文件操作
    /// </summary>
    internal class DeserializeManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RestoreFileData,
            DeserializeFileHeader,
            PrepareAssetList,
            DeserializeAssetList,
            PrepareBundleList,
            DeserializeBundleList,
            InitManifest,
            Done,
        }

        private readonly IManifestDecryptor _decryptor;
        private byte[] _sourceData;
        private BufferReader _buffer;
        private int _packageAssetCount;
        private int _packageBundleCount;
        private int _progressTotalValue;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 解析的清单实例
        /// </summary>
        public PackageManifest Manifest { get; private set; }

        /// <summary>
        /// 创建反序列化清单文件操作实例
        /// </summary>
        /// <param name="decryptor">清单解密器，为null时不解密</param>
        /// <param name="binaryData">清单二进制数据</param>
        public DeserializeManifestOperation(IManifestDecryptor decryptor, byte[] binaryData)
        {
            _decryptor = decryptor;
            _sourceData = binaryData;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.RestoreFileData;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RestoreFileData)
            {
                if (_decryptor != null)
                {
                    var resultData = _decryptor.Decrypt(_sourceData);
                    if (resultData != null)
                        _sourceData = resultData;
                }

                _buffer = new BufferReader(_sourceData);
                _steps = ESteps.DeserializeFileHeader;
            }

            if (_steps == ESteps.DeserializeFileHeader)
            {
                if (_buffer.IsValid == false)
                {
                    _steps = ESteps.Done;
                    SetError("Buffer is invalid.");
                    return;
                }

                // 读取文件标记
                uint fileMagic = _buffer.ReadUInt32();
                if (fileMagic != PackageManifestConsts.FileMagic)
                {
                    _steps = ESteps.Done;
                    SetError("Manifest file format is invalid.");
                    return;
                }

                // 读取文件版本
                int fileVersion = _buffer.ReadInt32();
                if (fileVersion != PackageManifestConsts.FileVersion)
                {
                    _steps = ESteps.Done;
                    SetError($"Manifest file version is not compatible: {fileVersion} != {PackageManifestConsts.FileVersion}.");
                    return;
                }

                // 读取文件头信息
                Manifest = new PackageManifest();
                Manifest.FileVersion = fileVersion;
                Manifest.EnableAddressable = _buffer.ReadBoolean();
                Manifest.SupportExtensionless = _buffer.ReadBoolean();
                Manifest.LocationToLower = _buffer.ReadBoolean();
                Manifest.IncludeAssetGuid = _buffer.ReadBoolean();
                Manifest.ReplaceAssetPathWithAddress = _buffer.ReadBoolean();
                Manifest.OutputNameStyle = _buffer.ReadInt32();
                Manifest.BuildBundleType = _buffer.ReadInt32();
                Manifest.BuildPipeline = _buffer.ReadString();
                Manifest.PackageName = _buffer.ReadString();
                Manifest.PackageVersion = _buffer.ReadString();
                Manifest.PackageNote = _buffer.ReadString();

                // 检测配置
                if (Manifest.EnableAddressable && Manifest.LocationToLower)
                    throw new YooManifestInvalidException("Addressable mode does not support converting locations to lowercase.");
                if (Manifest.EnableAddressable == false && Manifest.ReplaceAssetPathWithAddress)
                    throw new YooManifestInvalidException("Replacing asset path with address requires Addressable to be enabled.");

                _steps = ESteps.PrepareAssetList;
            }

            if (_steps == ESteps.PrepareAssetList)
            {
                _packageAssetCount = _buffer.ReadInt32();
                _progressTotalValue = _packageAssetCount;
                CreateAssetCollection(Manifest, _packageAssetCount);
                _steps = ESteps.DeserializeAssetList;
            }
            if (_steps == ESteps.DeserializeAssetList)
            {
                bool replaceAssetPath = false;
                if (UnityEngine.Application.isPlaying)
                {
                    if (Manifest.EnableAddressable && Manifest.ReplaceAssetPathWithAddress)
                        replaceAssetPath = true;
                }

                while (_packageAssetCount > 0)
                {
                    var packageAsset = new PackageAsset();
                    packageAsset.Address = _buffer.ReadString();
                    if (replaceAssetPath)
                    {
                        packageAsset.AssetPath = packageAsset.Address;
                        _buffer.SkipString(); //跳过解析AssetPath
                    }
                    else
                    {
                        packageAsset.AssetPath = _buffer.ReadString();
                    }
                    packageAsset.AssetGuid = _buffer.ReadString();
                    packageAsset.AssetTags = _buffer.ReadStringArray();
                    packageAsset.BundleID = _buffer.ReadInt32();
                    packageAsset.DependentBundleIDs = _buffer.ReadInt32Array();
                    FillAssetCollection(Manifest, packageAsset, replaceAssetPath);

                    _packageAssetCount--;
                    Progress = CalculateMultiStageProgress(0, 2, _packageAssetCount, _progressTotalValue);
                    if (IsBusy)
                        break;
                }

                if (_packageAssetCount <= 0)
                {
                    _steps = ESteps.PrepareBundleList;
                }
            }

            if (_steps == ESteps.PrepareBundleList)
            {
                _packageBundleCount = _buffer.ReadInt32();
                _progressTotalValue = _packageBundleCount;
                CreateBundleCollection(Manifest, _packageBundleCount);
                _steps = ESteps.DeserializeBundleList;
            }
            if (_steps == ESteps.DeserializeBundleList)
            {
                while (_packageBundleCount > 0)
                {
                    var packageBundle = new PackageBundle();
                    packageBundle.BundleName = _buffer.ReadString();
                    packageBundle.UnityCrc = _buffer.ReadUInt32();
                    packageBundle.FileHash = _buffer.ReadString();
                    packageBundle.FileCrc = _buffer.ReadUInt32();
                    packageBundle.FileSize = _buffer.ReadInt64();
                    packageBundle.IsEncrypted = _buffer.ReadBoolean();
                    packageBundle.Tags = _buffer.ReadStringArray();
                    packageBundle.DependentBundleIDs = _buffer.ReadInt32Array();
                    FillBundleCollection(Manifest, packageBundle);

                    _packageBundleCount--;
                    Progress = CalculateMultiStageProgress(1, 2, _packageBundleCount, _progressTotalValue);
                    if (IsBusy)
                        break;
                }

                if (_packageBundleCount <= 0)
                {
                    _steps = ESteps.InitManifest;
                }
            }

            if (_steps == ESteps.InitManifest)
            {
                Manifest.Initialize();
                _steps = ESteps.Done;
                SetResult();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        private void CreateAssetCollection(PackageManifest manifest, int assetCount)
        {
            manifest.AssetList = new List<PackageAsset>(assetCount);
            manifest.AssetDictionary = new Dictionary<string, PackageAsset>(assetCount);

            if (manifest.EnableAddressable)
            {
                manifest.AssetPathsByLocation = new Dictionary<string, string>(assetCount * 3);
            }
            else
            {
                if (manifest.LocationToLower)
                    manifest.AssetPathsByLocation = new Dictionary<string, string>(assetCount * 2, StringComparer.OrdinalIgnoreCase);
                else
                    manifest.AssetPathsByLocation = new Dictionary<string, string>(assetCount * 2);
            }

            if (manifest.IncludeAssetGuid)
                manifest.AssetPathsByGuid = new Dictionary<string, string>(assetCount);
            else
                manifest.AssetPathsByGuid = new Dictionary<string, string>();
        }
        private void FillAssetCollection(PackageManifest manifest, PackageAsset packageAsset, bool replaceAssetPath)
        {
            // 添加到列表集合
            manifest.AssetList.Add(packageAsset);

            // 注意：我们不允许原始路径存在重名
            string assetPath = packageAsset.AssetPath;
#if UNITY_EDITOR || DEBUG
            if (manifest.AssetDictionary.ContainsKey(assetPath))
                throw new YooManifestInvalidException($"Asset path already exists: '{assetPath}'.");
#endif
            manifest.AssetDictionary.Add(assetPath, packageAsset);

            // 填充AssetPathMapping1
            {
                string location = packageAsset.AssetPath;

                // 添加原生路径的映射
#if UNITY_EDITOR || DEBUG
                if (manifest.AssetPathsByLocation.ContainsKey(location))
                    throw new YooManifestInvalidException($"Location already exists: '{location}'.");
#endif
                manifest.AssetPathsByLocation.Add(location, packageAsset.AssetPath);

                // 添加无后缀名路径的映射
                if (manifest.SupportExtensionless)
                {
                    string locationWithoutExtension = Path.ChangeExtension(location, null);
                    if (ReferenceEquals(location, locationWithoutExtension) == false)
                    {
                        if (manifest.AssetPathsByLocation.ContainsKey(locationWithoutExtension))
                            YooLogger.LogWarning($"Location already exists: '{locationWithoutExtension}'.");
                        else
                            manifest.AssetPathsByLocation.Add(locationWithoutExtension, packageAsset.AssetPath);
                    }
                }
            }

            // 填充AssetPathMapping2
            if (manifest.IncludeAssetGuid)
            {
#if UNITY_EDITOR || DEBUG
                if (manifest.AssetPathsByGuid.ContainsKey(packageAsset.AssetGuid))
                    throw new YooManifestInvalidException($"Asset GUID already exists: '{packageAsset.AssetGuid}'.");
#endif
                manifest.AssetPathsByGuid.Add(packageAsset.AssetGuid, packageAsset.AssetPath);
            }

            // 添加可寻址地址
            if (manifest.EnableAddressable && replaceAssetPath == false)
            {
                string location = packageAsset.Address;
                if (string.IsNullOrEmpty(location) == false)
                {
#if UNITY_EDITOR || DEBUG
                    if (manifest.AssetPathsByLocation.ContainsKey(location))
                        throw new YooManifestInvalidException($"Location already exists: '{location}'.");
#endif
                    manifest.AssetPathsByLocation.Add(location, packageAsset.AssetPath);
                }
            }
        }

        private void CreateBundleCollection(PackageManifest manifest, int bundleCount)
        {
            manifest.BundleList = new List<PackageBundle>(bundleCount);
            manifest.BundlesByBundleName = new Dictionary<string, PackageBundle>(bundleCount);
            manifest.BundlesByFileName = new Dictionary<string, PackageBundle>(bundleCount);
            manifest.BundlesByGuid = new Dictionary<string, PackageBundle>(bundleCount);
        }
        private void FillBundleCollection(PackageManifest manifest, PackageBundle packageBundle)
        {
            // 初始化资源包
            packageBundle.Initialize(manifest);

            // 添加到列表集合
            manifest.BundleList.Add(packageBundle);

            manifest.BundlesByBundleName.Add(packageBundle.BundleName, packageBundle);
            manifest.BundlesByFileName.Add(packageBundle.GetFileName(), packageBundle);
            manifest.BundlesByGuid.Add(packageBundle.BundleGuid, packageBundle);
        }
    }
}