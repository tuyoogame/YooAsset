﻿using System.IO;
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
        private int _packageAssetCount;
        private int _packageBundleCount;
        private int _progressTotalValue;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 解析的清单实例
        /// </summary>
        public PackageManifest Manifest { private set; get; }

        public DeserializeManifestOperation(byte[] binaryData)
        {
            _buffer = new BufferReader(binaryData);
        }
        internal override void InternalOnStart()
        {
            _steps = ESteps.DeserializeFileHeader;
        }
        internal override void InternalOnUpdate()
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
                    if (fileSign != YooAssetSettings.ManifestFileSign)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "The manifest file format is invalid !";
                        return;
                    }

                    // 读取文件版本
                    string fileVersion = _buffer.ReadUTF8();
                    if (fileVersion != YooAssetSettings.ManifestFileVersion)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"The manifest file version are not compatible : {fileVersion} != {YooAssetSettings.ManifestFileVersion}";
                        return;
                    }

                    // 读取文件头信息
                    Manifest = new PackageManifest();
                    Manifest.FileVersion = fileVersion;
                    Manifest.EnableAddressable = _buffer.ReadBool();
                    Manifest.LocationToLower = _buffer.ReadBool();
                    Manifest.IncludeAssetGUID = _buffer.ReadBool();
                    Manifest.OutputNameStyle = _buffer.ReadInt32();
                    Manifest.BuildPipeline = _buffer.ReadUTF8();
                    Manifest.PackageName = _buffer.ReadUTF8();
                    Manifest.PackageVersion = _buffer.ReadUTF8();

                    // 检测配置
                    if (Manifest.EnableAddressable && Manifest.LocationToLower)
                        throw new System.Exception("Addressable not support location to lower !");

                    _steps = ESteps.PrepareAssetList;
                }

                if (_steps == ESteps.PrepareAssetList)
                {
                    _packageAssetCount = _buffer.ReadInt32();
                    Manifest.AssetList = new List<PackageAsset>(_packageAssetCount);
                    Manifest.AssetDic = new Dictionary<string, PackageAsset>(_packageAssetCount);

                    if (Manifest.EnableAddressable)
                        Manifest.AssetPathMapping1 = new Dictionary<string, string>(_packageAssetCount * 3);
                    else
                        Manifest.AssetPathMapping1 = new Dictionary<string, string>(_packageAssetCount * 2);

                    if (Manifest.IncludeAssetGUID)
                        Manifest.AssetPathMapping2 = new Dictionary<string, string>(_packageAssetCount);
                    else
                        Manifest.AssetPathMapping2 = new Dictionary<string, string>();

                    _progressTotalValue = _packageAssetCount;
                    _steps = ESteps.DeserializeAssetList;
                }
                if (_steps == ESteps.DeserializeAssetList)
                {
                    while (_packageAssetCount > 0)
                    {
                        var packageAsset = new PackageAsset();
                        packageAsset.Address = _buffer.ReadUTF8();
                        packageAsset.AssetPath = _buffer.ReadUTF8();
                        packageAsset.AssetGUID = _buffer.ReadUTF8();
                        packageAsset.AssetTags = _buffer.ReadUTF8Array();
                        packageAsset.BundleID = _buffer.ReadInt32();
                        Manifest.AssetList.Add(packageAsset);

                        // 注意：我们不允许原始路径存在重名
                        string assetPath = packageAsset.AssetPath;
                        if (!Manifest.AssetDic.TryAdd(assetPath, packageAsset))
                            throw new System.Exception($"AssetPath have existed : {assetPath}");

                        // 填充AssetPathMapping1
                        {
                            string location = packageAsset.AssetPath;
                            if (Manifest.LocationToLower)
                                location = location.ToLower();

                            // 添加原生路径的映射
                            if (!Manifest.AssetPathMapping1.TryAdd(location, packageAsset.AssetPath))
                                throw new System.Exception($"Location have existed : {location}");

                            // 添加无后缀名路径的映射
                            string locationWithoutExtension = Path.ChangeExtension(location, null);
                            if (ReferenceEquals(location, locationWithoutExtension) == false)
                            {
                                if (!Manifest.AssetPathMapping1.TryAdd(locationWithoutExtension, packageAsset.AssetPath))
                                    YooLogger.Warning($"Location have existed : {locationWithoutExtension}");
                            }
                        }
                        if (Manifest.EnableAddressable)
                        {
                            string location = packageAsset.Address;
                            if (string.IsNullOrEmpty(location) == false)
                            {
                                if (!Manifest.AssetPathMapping1.TryAdd(location, packageAsset.AssetPath))
                                    throw new System.Exception($"Location have existed : {location}");
                            }
                        }

                        // 填充AssetPathMapping2
                        if (Manifest.IncludeAssetGUID)
                        {
                            if (!Manifest.AssetPathMapping2.TryAdd(packageAsset.AssetGUID, packageAsset.AssetPath))
                                throw new System.Exception($"AssetGUID have existed : {packageAsset.AssetGUID}");
                        }

                        _packageAssetCount--;
                        Progress = 1f - _packageAssetCount / _progressTotalValue;
                        if (OperationSystem.IsBusy)
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
                    Manifest.BundleList = new List<PackageBundle>(_packageBundleCount);
                    Manifest.BundleDic1 = new Dictionary<string, PackageBundle>(_packageBundleCount);
                    Manifest.BundleDic2 = new Dictionary<string, PackageBundle>(_packageBundleCount);
                    Manifest.BundleDic3 = new Dictionary<string, PackageBundle>(_packageBundleCount);
                    _progressTotalValue = _packageBundleCount;
                    _steps = ESteps.DeserializeBundleList;
                }
                if (_steps == ESteps.DeserializeBundleList)
                {
                    while (_packageBundleCount > 0)
                    {
                        var packageBundle = new PackageBundle();
                        packageBundle.BundleName = _buffer.ReadUTF8();
                        packageBundle.UnityCRC = _buffer.ReadUInt32();
                        packageBundle.FileHash = _buffer.ReadUTF8();
                        packageBundle.FileCRC = _buffer.ReadUTF8();
                        packageBundle.FileSize = _buffer.ReadInt64();
                        packageBundle.Encrypted = _buffer.ReadBool();
                        packageBundle.Tags = _buffer.ReadUTF8Array();
                        packageBundle.DependIDs = _buffer.ReadInt32Array();
                        packageBundle.ParseBundle(Manifest);
                        Manifest.BundleList.Add(packageBundle);
                        Manifest.BundleDic1.Add(packageBundle.BundleName, packageBundle);
                        Manifest.BundleDic2.Add(packageBundle.FileName, packageBundle);

                        // 注意：原始文件可能存在相同的CacheGUID
                        if (Manifest.BundleDic3.ContainsKey(packageBundle.CacheGUID) == false)
                            Manifest.BundleDic3.Add(packageBundle.CacheGUID, packageBundle);

                        _packageBundleCount--;
                        Progress = 1f - _packageBundleCount / _progressTotalValue;
                        if (OperationSystem.IsBusy)
                            break;
                    }

                    if (_packageBundleCount <= 0)
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