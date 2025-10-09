using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    [Serializable]
    public class ReportSummary
    {
        /// <summary>
        /// YooAsset版本
        /// </summary>
        public string YooVersion;

        /// <summary>
        /// 引擎版本
        /// </summary>
        public string UnityVersion;

        /// <summary>
        /// 构建时间
        /// </summary>
        public string BuildDate;

        /// <summary>
        /// 构建耗时（单位：秒）
        /// </summary>
        public int BuildSeconds;

        /// <summary>
        /// 构建平台
        /// </summary>
        public BuildTarget BuildTarget;

        /// <summary>
        /// 构建管线
        /// </summary>
        public string BuildPipeline;

        /// <summary>
        /// 构建的资源包类型
        /// </summary>
        public int BuildBundleType;

        /// <summary>
        /// 构建包裹名称
        /// </summary>
        public string BuildPackageName;

        /// <summary>
        /// 构建包裹版本
        /// </summary>
        public string BuildPackageVersion;

        /// <summary>
        /// 构建包裹备注
        /// </summary>
        public string BuildPackageNote;

        // 收集器配置
        public bool UniqueBundleName;
        public bool EnableAddressable;
        public bool SupportExtensionless;
        public bool LocationToLower;
        public bool IncludeAssetGUID;
        public bool AutoCollectShaders;
        public string IgnoreRuleName;

        // 构建参数
        public bool ClearBuildCacheFiles;
        public bool UseAssetDependencyDB;
        public bool EnableSharePackRule;
        public bool SingleReferencedPackAlone;
        public string EncryptionServicesClassName;
        public string ManifestProcessServicesClassName;
        public string ManifestRestoreServicesClassName;
        public EFileNameStyle FileNameStyle;
        
        // 引擎参数
        public ECompressOption CompressOption;
        public bool DisableWriteTypeTree;
        public bool IgnoreTypeTreeChanges;
        public bool ReplaceAssetPathWithAddress;
        public bool WriteLinkXML = true;
        public string CacheServerHost;
        public int CacheServerPort;
        public string BuiltinShadersBundleName;
        public string MonoScriptsBundleName;

        // 构建结果
        public int AssetFileTotalCount;
        public int MainAssetTotalCount;
        public int AllBundleTotalCount;
        public long AllBundleTotalSize;
        public int EncryptedBundleTotalCount;
        public long EncryptedBundleTotalSize;
    }
}