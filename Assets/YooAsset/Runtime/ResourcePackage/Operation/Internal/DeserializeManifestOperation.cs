using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;

namespace YooAsset
{
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

        private readonly IManifestRestoreServices _services;
        private byte[] _sourceData;
        private BufferReader _buffer;
        private int _packageAssetCount;
        private int _packageBundleCount;
        private int _progressTotalValue;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 解析的清单实例
        /// </summary>
        public PackageManifest Manifest { private set; get; }

        public DeserializeManifestOperation(IManifestRestoreServices services, byte[] binaryData)
        {
            _services = services;
            _sourceData = binaryData;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.RestoreFileData;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            try
            {
                if (_steps == ESteps.RestoreFileData)
                {
                    if (_services != null)
                    {
                        var resultData = _services.RestoreManifest(_sourceData);
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
                        Status = EOperationStatus.Failed;
                        Error = "Buffer is invalid !";
                        return;
                    }

                    // 读取文件标记
                    uint fileSign = _buffer.ReadUInt32();
                    if (fileSign != ManifestDefine.FileSign)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = "The manifest file format is invalid !";
                        return;
                    }

                    // 读取文件版本
                    string fileVersion = _buffer.ReadUTF8();
                    Version fileVer = new Version(fileVersion);
                    Version ver2025_8_28 = new Version(ManifestDefine.VERSION_2025_8_28);
                    Version ver2025_9_30 = new Version(ManifestDefine.VERSION_2025_9_30);
                    if (fileVer < ver2025_8_28)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"The manifest file version are not compatible : {fileVersion} != {ManifestDefine.FileVersion}";
                        return;
                    }

                    // 读取文件头信息
                    Manifest = new PackageManifest();
                    Manifest.FileVersion = fileVersion;
                    Manifest.EnableAddressable = _buffer.ReadBool();
                    Manifest.SupportExtensionless = _buffer.ReadBool();
                    Manifest.LocationToLower = _buffer.ReadBool();
                    Manifest.IncludeAssetGUID = _buffer.ReadBool();
                    if (fileVer >= ver2025_9_30)
                        Manifest.ReplaceAssetPathWithAddress = _buffer.ReadBool();
                    else
                        Manifest.ReplaceAssetPathWithAddress = false;
                    Manifest.OutputNameStyle = _buffer.ReadInt32();
                    Manifest.BuildBundleType = _buffer.ReadInt32();
                    Manifest.BuildPipeline = _buffer.ReadUTF8();
                    Manifest.PackageName = _buffer.ReadUTF8();
                    Manifest.PackageVersion = _buffer.ReadUTF8();
                    Manifest.PackageNote = _buffer.ReadUTF8();

                    // 检测配置
                    if (Manifest.EnableAddressable && Manifest.LocationToLower)
                        throw new System.Exception("Addressable not support location to lower !");
                    if (Manifest.EnableAddressable == false && Manifest.ReplaceAssetPathWithAddress)
                        throw new System.Exception("Replace asset path with address need enable Addressable !");

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
                        packageAsset.Address = _buffer.ReadUTF8();
                        if (replaceAssetPath)
                        {
                            packageAsset.AssetPath = packageAsset.Address;
                            _buffer.SkipUTF8(); //跳过解析AssetPath
                        }
                        else
                        {
                            packageAsset.AssetPath = _buffer.ReadUTF8();
                        }
                        packageAsset.AssetGUID = _buffer.ReadUTF8();
                        packageAsset.AssetTags = _buffer.ReadUTF8Array();
                        packageAsset.BundleID = _buffer.ReadInt32();
                        packageAsset.DependBundleIDs = _buffer.ReadInt32Array();
                        FillAssetCollection(Manifest, packageAsset, replaceAssetPath);

                        _packageAssetCount--;
                        Progress = 1f - _packageAssetCount / _progressTotalValue;
                        if (IsWaitForAsyncComplete == false)
                        {
                            if (OperationSystem.IsBusy)
                                break;
                        }
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
                        packageBundle.BundleName = _buffer.ReadUTF8();
                        packageBundle.UnityCRC = _buffer.ReadUInt32();
                        packageBundle.FileHash = _buffer.ReadUTF8();
                        packageBundle.FileCRC = _buffer.ReadUInt32();
                        packageBundle.FileSize = _buffer.ReadInt64();
                        packageBundle.Encrypted = _buffer.ReadBool();
                        packageBundle.Tags = _buffer.ReadUTF8Array();
                        packageBundle.DependBundleIDs = _buffer.ReadInt32Array();
                        FillBundleCollection(Manifest, packageBundle);

                        _packageBundleCount--;
                        Progress = 1f - _packageBundleCount / _progressTotalValue;
                        if (IsWaitForAsyncComplete == false)
                        {
                            if (OperationSystem.IsBusy)
                                break;
                        }
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
                    Status = EOperationStatus.Succeed;
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
        internal override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        private void CreateAssetCollection(PackageManifest manifest, int assetCount)
        {
            manifest.AssetList = new List<PackageAsset>(assetCount);
            manifest.AssetDic = new Dictionary<string, PackageAsset>(assetCount);

            if (manifest.EnableAddressable)
            {
                manifest.AssetPathMapping1 = new Dictionary<string, string>(assetCount * 3);
            }
            else
            {
                if (manifest.LocationToLower)
                    manifest.AssetPathMapping1 = new Dictionary<string, string>(assetCount * 2, StringComparer.OrdinalIgnoreCase);
                else
                    manifest.AssetPathMapping1 = new Dictionary<string, string>(assetCount * 2);
            }

            if (manifest.IncludeAssetGUID)
                manifest.AssetPathMapping2 = new Dictionary<string, string>(assetCount);
            else
                manifest.AssetPathMapping2 = new Dictionary<string, string>();
        }
        private void FillAssetCollection(PackageManifest manifest, PackageAsset packageAsset, bool replaceAssetPath)
        {
            // 添加到列表集合
            manifest.AssetList.Add(packageAsset);

            // 注意：我们不允许原始路径存在重名
            string assetPath = packageAsset.AssetPath;
            if (manifest.AssetDic.ContainsKey(assetPath))
                throw new System.Exception($"AssetPath have existed : {assetPath}");
            else
                manifest.AssetDic.Add(assetPath, packageAsset);

            // 填充AssetPathMapping1
            {
                string location = packageAsset.AssetPath;

                // 添加原生路径的映射
                if (manifest.AssetPathMapping1.ContainsKey(location))
                    throw new System.Exception($"Location have existed : {location}");
                else
                    manifest.AssetPathMapping1.Add(location, packageAsset.AssetPath);

                // 添加无后缀名路径的映射
                if (manifest.SupportExtensionless)
                {
                    string locationWithoutExtension = Path.ChangeExtension(location, null);
                    if (ReferenceEquals(location, locationWithoutExtension) == false)
                    {
                        if (manifest.AssetPathMapping1.ContainsKey(locationWithoutExtension))
                            YooLogger.Warning($"Location have existed : {locationWithoutExtension}");
                        else
                            manifest.AssetPathMapping1.Add(locationWithoutExtension, packageAsset.AssetPath);
                    }
                }
            }

            // 填充AssetPathMapping2
            if (manifest.IncludeAssetGUID)
            {
                if (manifest.AssetPathMapping2.ContainsKey(packageAsset.AssetGUID))
                    throw new System.Exception($"AssetGUID have existed : {packageAsset.AssetGUID}");
                else
                    manifest.AssetPathMapping2.Add(packageAsset.AssetGUID, packageAsset.AssetPath);
            }

            // 添加可寻址地址
            if (manifest.EnableAddressable && replaceAssetPath == false)
            {
                string location = packageAsset.Address;
                if (string.IsNullOrEmpty(location) == false)
                {
                    if (manifest.AssetPathMapping1.ContainsKey(location))
                        throw new System.Exception($"Location have existed : {location}");
                    else
                        manifest.AssetPathMapping1.Add(location, packageAsset.AssetPath);
                }
            }
        }

        private void CreateBundleCollection(PackageManifest manifest, int bundleCount)
        {
            manifest.BundleList = new List<PackageBundle>(bundleCount);
            manifest.BundleDic1 = new Dictionary<string, PackageBundle>(bundleCount);
            manifest.BundleDic2 = new Dictionary<string, PackageBundle>(bundleCount);
            manifest.BundleDic3 = new Dictionary<string, PackageBundle>(bundleCount);
        }
        private void FillBundleCollection(PackageManifest manifest, PackageBundle packageBundle)
        {
            // 初始化资源包
            packageBundle.InitBundle(manifest);

            // 添加到列表集合
            manifest.BundleList.Add(packageBundle);

            manifest.BundleDic1.Add(packageBundle.BundleName, packageBundle);
            manifest.BundleDic2.Add(packageBundle.FileName, packageBundle);
            manifest.BundleDic3.Add(packageBundle.BundleGUID, packageBundle);
        }
    }
}