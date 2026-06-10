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
            DecryptManifest,
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
        private string[] _tagTable;
        private string[] _directoryTable;
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
            _steps = ESteps.DecryptManifest;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.DecryptManifest)
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

                // 读取全局标签表
                _tagTable = _buffer.ReadStringArray();

                // 读取全局目录表
                _directoryTable = _buffer.ReadStringArray();

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

                    // Address
                    if (Manifest.EnableAddressable)
                        packageAsset.Address = _buffer.ReadString();
                    else
                        packageAsset.Address = string.Empty;

                    // AssetPath
                    ushort dirIndex = _buffer.ReadUInt16();
                    if (replaceAssetPath)
                    {
                        packageAsset.AssetPath = packageAsset.Address;
                        _buffer.SkipString(); //跳过解析文件名
                    }
                    else
                    {
                        packageAsset.AssetPath = ResolvePath(dirIndex);
                    }

                    // AssetGuid
                    if (Manifest.IncludeAssetGuid)
                        packageAsset.AssetGuid = _buffer.ReadHash16();
                    else
                        packageAsset.AssetGuid = string.Empty;

                    // Tags
                    var tagIndices = _buffer.ReadUInt16Array();
                    packageAsset.Tags = ResolveTags(tagIndices);

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
                    packageBundle.FileHash = _buffer.ReadHash16();
                    packageBundle.FileCrc = _buffer.ReadUInt32();
                    packageBundle.FileSize = _buffer.ReadInt64();
                    packageBundle.IsEncrypted = _buffer.ReadBoolean();
                    packageBundle.Tags = ResolveTags(_buffer.ReadUInt16Array());
                    packageBundle.DependentBundleIDs = _buffer.ReadInt32Array();
                    packageBundle.Initialize(Manifest);
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
                FillBundleMainAssets(Manifest);
                FillBundleReferrerBundleIDs(Manifest);

                _steps = ESteps.Done;
                SetResult();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }

        private PackageTags ResolveTags(ushort[] tagIndices)
        {
            if (tagIndices.Length == 0)
                return new PackageTags(Array.Empty<string>());

            string[] tags = new string[tagIndices.Length];
            for (int i = 0; i < tagIndices.Length; i++)
            {
                ushort tagIndex = tagIndices[i];

#if UNITY_EDITOR || DEBUG
                if (tagIndex >= _tagTable.Length)
                    throw new YooManifestInvalidException($"Invalid tag index: {tagIndex}. Valid range is 0 to {_tagTable.Length - 1}.");
#endif

                tags[i] = _tagTable[tagIndex];
            }

            return new PackageTags(tags);
        }
        private string ResolvePath(ushort dirIndex)
        {
#if UNITY_EDITOR || DEBUG
            if (dirIndex >= _directoryTable.Length)
                throw new YooManifestInvalidException($"Invalid directory index: {dirIndex}. Valid range is 0 to {_directoryTable.Length - 1}.");
#endif

            string fileName = _buffer.ReadString();
            string directory = _directoryTable[dirIndex];
            if (string.IsNullOrEmpty(directory))
                return fileName;
            else
                return string.Concat(directory, "/", fileName);
        }

        private void CreateAssetCollection(PackageManifest manifest, int assetCount)
        {
            manifest.AssetList = new List<PackageAsset>(assetCount);

            if (manifest.EnableAddressable)
            {
                manifest.AssetsByLocation = new Dictionary<string, PackageAsset>(assetCount * 3);
            }
            else
            {
                if (manifest.LocationToLower)
                    manifest.AssetsByLocation = new Dictionary<string, PackageAsset>(assetCount * 2, StringComparer.OrdinalIgnoreCase);
                else
                    manifest.AssetsByLocation = new Dictionary<string, PackageAsset>(assetCount * 2);
            }

            if (manifest.IncludeAssetGuid)
                manifest.AssetsByGuid = new Dictionary<string, PackageAsset>(assetCount);
            else
                manifest.AssetsByGuid = new Dictionary<string, PackageAsset>();
        }
        private void FillAssetCollection(PackageManifest manifest, PackageAsset packageAsset, bool replaceAssetPath)
        {
            // 添加到列表集合
            manifest.AssetList.Add(packageAsset);

            // 填充AssetsByLocation
            {
                string location = packageAsset.AssetPath;

                // 添加原生路径的映射（注意：我们不允许原始路径存在重名）
#if UNITY_EDITOR || DEBUG
                if (manifest.AssetsByLocation.ContainsKey(location))
                    throw new YooManifestInvalidException($"Asset path already exists: '{location}'.");
#endif
                manifest.AssetsByLocation.Add(location, packageAsset);

                // 添加无后缀名路径的映射
                if (manifest.SupportExtensionless)
                {
                    string locationWithoutExtension = Path.ChangeExtension(location, null);
                    if (ReferenceEquals(location, locationWithoutExtension) == false)
                    {
                        if (manifest.AssetsByLocation.ContainsKey(locationWithoutExtension))
                            YooLogger.LogWarning($"Location already exists: '{locationWithoutExtension}'.");
                        else
                            manifest.AssetsByLocation.Add(locationWithoutExtension, packageAsset);
                    }
                }
            }

            // 填充AssetsByGuid
            if (manifest.IncludeAssetGuid)
            {
#if UNITY_EDITOR || DEBUG
                if (manifest.AssetsByGuid.ContainsKey(packageAsset.AssetGuid))
                    throw new YooManifestInvalidException($"Asset GUID already exists: '{packageAsset.AssetGuid}'.");
#endif
                manifest.AssetsByGuid.Add(packageAsset.AssetGuid, packageAsset);
            }

            // 添加可寻址地址
            if (manifest.EnableAddressable && replaceAssetPath == false)
            {
                string location = packageAsset.Address;
                if (string.IsNullOrEmpty(location) == false)
                {
#if UNITY_EDITOR || DEBUG
                    if (manifest.AssetsByLocation.ContainsKey(location))
                        throw new YooManifestInvalidException($"Location already exists: '{location}'.");
#endif
                    manifest.AssetsByLocation.Add(location, packageAsset);
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
            // 添加到列表集合
            manifest.BundleList.Add(packageBundle);

            manifest.BundlesByBundleName.Add(packageBundle.BundleName, packageBundle);
            manifest.BundlesByFileName.Add(packageBundle.GetFileName(), packageBundle);
            manifest.BundlesByGuid.Add(packageBundle.BundleGuid, packageBundle);
        }

        private void FillBundleMainAssets(PackageManifest manifest)
        {
            int bundleCount = manifest.BundleList.Count;

            // 1. 统计每个资源包的主资源数量
            int[] mainAssetCounts = new int[bundleCount];
            foreach (var packageAsset in manifest.AssetList)
            {
                int bundleID = packageAsset.BundleID;
                if (bundleID < 0 || bundleID >= bundleCount)
                    throw new ArgumentOutOfRangeException($"Invalid bundle ID: {bundleID}. Valid range is 0 to {bundleCount - 1}.");

                mainAssetCounts[bundleID]++;
            }

            // 2. 创建列表
            for (int index = 0; index < bundleCount; index++)
            {
                int capacity = mainAssetCounts[index];
                manifest.BundleList[index].MainAssets = new List<PackageAsset>(capacity);
            }

            // 3. 填充数据
            foreach (var packageAsset in manifest.AssetList)
            {
                manifest.BundleList[packageAsset.BundleID].MainAssets.Add(packageAsset);
            }
        }
        private void FillBundleReferrerBundleIDs(PackageManifest manifest)
        {
            int bundleCount = manifest.BundleList.Count;

            // 1. 统计每个资源包被引用的次数
            int[] referrerCounts = new int[bundleCount];
            for (int index = 0; index < bundleCount; index++)
            {
                foreach (int dependIndex in manifest.BundleList[index].DependentBundleIDs)
                {
                    if (dependIndex < 0 || dependIndex >= bundleCount)
                        throw new ArgumentOutOfRangeException($"Invalid dependent bundle index: {dependIndex}. Valid range is 0 to {bundleCount - 1}.");

                    referrerCounts[dependIndex]++;
                }
            }

            // 2. 创建列表
            for (int index = 0; index < bundleCount; index++)
            {
                int capacity = referrerCounts[index];
                manifest.BundleList[index].ReferrerBundleIDs = new List<int>(capacity);
            }

            // 3. 填充数据
            for (int index = 0; index < bundleCount; index++)
            {
                foreach (int dependIndex in manifest.BundleList[index].DependentBundleIDs)
                {
                    var dependBundle = manifest.BundleList[dependIndex];
#if UNITY_EDITOR || DEBUG
                    if (dependBundle.ReferrerBundleIDs.Contains(index))
                        throw new YooManifestInvalidException($"Duplicate referrer bundle ID detected: referrer {index} -> bundle {dependIndex}.");
#endif
                    dependBundle.ReferrerBundleIDs.Add(index);
                }
            }
        }
    }
}