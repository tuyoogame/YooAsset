namespace YooAsset
{
    /// <summary>
    /// 序列化清单文件操作
    /// </summary>
    internal class SerializeManifestOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            SerializeFileHeader,
            SerializeAssetList,
            SerializeBundleList,
            EncryptManifest,
            Done,
        }

        private readonly BuildManifest _manifest;
        private readonly IManifestEncryptor _encryptor;
        private BufferWriter _buffer;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 序列化后的二进制数据
        /// </summary>
        public byte[] FileData { get; private set; }

        /// <summary>
        /// 创建序列化清单文件操作实例
        /// </summary>
        /// <param name="manifest">清单对象</param>
        /// <param name="encryptor">清单加密器，为null时不加密</param>
        public SerializeManifestOperation(BuildManifest manifest, IManifestEncryptor encryptor)
        {
            _manifest = manifest;
            _encryptor = encryptor;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.SerializeFileHeader;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.SerializeFileHeader)
            {
                // 创建缓存器
                _buffer = new BufferWriter(PackageManifestConsts.MaxFileSize);

                // 写入文件标记
                _buffer.WriteUInt32(PackageManifestConsts.FileMagic);

                // 写入文件版本
                _buffer.WriteInt32(_manifest.FileVersion);

                // 写入文件头信息
                _buffer.WriteBoolean(_manifest.EnableAddressable);
                _buffer.WriteBoolean(_manifest.SupportExtensionless);
                _buffer.WriteBoolean(_manifest.LocationToLower);
                _buffer.WriteBoolean(_manifest.IncludeAssetGuid);
                _buffer.WriteBoolean(_manifest.ReplaceAssetPathWithAddress);
                _buffer.WriteInt32(_manifest.OutputNameStyle);
                _buffer.WriteInt32(_manifest.BuildBundleType);
                _buffer.WriteString(_manifest.BuildPipeline);
                _buffer.WriteString(_manifest.PackageName);
                _buffer.WriteString(_manifest.PackageVersion);
                _buffer.WriteString(_manifest.PackageNote);

                // 写入全局标签表
                _buffer.WriteStringArray(_manifest.TagTable);

                // 写入全局目录表
                _buffer.WriteStringArray(_manifest.DirectoryTable);

                _steps = ESteps.SerializeAssetList;
            }

            if (_steps == ESteps.SerializeAssetList)
            {
                _buffer.WriteInt32(_manifest.AssetList.Count);
                for (int i = 0; i < _manifest.AssetList.Count; i++)
                {
                    var buildAsset = _manifest.AssetList[i];

                    // Address
                    if (_manifest.EnableAddressable)
                        _buffer.WriteString(buildAsset.Address);

                    // AssetPath
                    BuildManifest.SplitAssetPath(buildAsset.AssetPath, out string directory, out string fileName);
                    ushort directoryIndex = _manifest.ConvertDirectoryToIndex(directory);
                    _buffer.WriteUInt16(directoryIndex);
                    _buffer.WriteString(fileName);

                    // AssetGuid
                    if (_manifest.IncludeAssetGuid)
                        _buffer.WriteHash16(buildAsset.AssetGuid);

                    // Tags
                    ushort[] tagIndices = _manifest.ConvertTagsToIndices(buildAsset.Tags);
                    _buffer.WriteUInt16Array(tagIndices);

                    _buffer.WriteInt32(buildAsset.BundleID);
                    _buffer.WriteInt32Array(buildAsset.DependentBundleIDs);
                }

                _steps = ESteps.SerializeBundleList;
            }

            if (_steps == ESteps.SerializeBundleList)
            {
                _buffer.WriteInt32(_manifest.BundleList.Count);
                for (int i = 0; i < _manifest.BundleList.Count; i++)
                {
                    var buildBundle = _manifest.BundleList[i];
                    ushort[] tagIndices = _manifest.ConvertTagsToIndices(buildBundle.Tags);
                    _buffer.WriteString(buildBundle.BundleName);
                    _buffer.WriteUInt32(buildBundle.UnityCrc);
                    _buffer.WriteHash16(buildBundle.FileHash);
                    _buffer.WriteUInt32(buildBundle.FileCrc);
                    _buffer.WriteInt64(buildBundle.FileSize);
                    _buffer.WriteBoolean(buildBundle.IsEncrypted);
                    _buffer.WriteUInt16Array(tagIndices);
                    _buffer.WriteInt32Array(buildBundle.DependentBundleIDs);
                }

                _steps = ESteps.EncryptManifest;
            }

            if (_steps == ESteps.EncryptManifest)
            {
                if (_encryptor != null)
                {
                    var tempBytes = _buffer.ToArray();
                    FileData = _encryptor.Encrypt(tempBytes);
                }
                else
                {
                    FileData = _buffer.ToArray();
                }

                _steps = ESteps.Done;
                SetResult();
            }
        }
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
    }
}
