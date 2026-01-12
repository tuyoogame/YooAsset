using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 构建报告的汇总信息
    /// </summary>
    [Serializable]
    public class ReportSummary
    {
        /// <summary>
        /// YooAsset 版本
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

        #region 收集器配置
        /// <summary>
        /// 是否启用唯一资源包名称
        /// </summary>
        public bool UniqueBundleName;

        /// <summary>
        /// 是否启用可寻址功能
        /// </summary>
        public bool EnableAddressable;

        /// <summary>
        /// 是否支持无扩展名加载
        /// </summary>
        public bool SupportExtensionless;

        /// <summary>
        /// 是否将资源定位地址转为小写
        /// </summary>
        public bool LocationToLower;

        /// <summary>
        /// 是否在清单中包含资源 Guid
        /// </summary>
        public bool IncludeAssetGuid;

        /// <summary>
        /// 是否自动收集 Shader 资源
        /// </summary>
        public bool AutoCollectShaders;

        /// <summary>
        /// 忽略规则名称
        /// </summary>
        public string IgnoreRuleName;
        #endregion

        #region 构建参数
        /// <summary>
        /// 是否清除构建缓存文件
        /// </summary>
        public bool ClearBuildCacheFiles;

        /// <summary>
        /// 是否使用资源依赖数据库
        /// </summary>
        public bool UseAssetDependencyDB;

        /// <summary>
        /// 是否启用共享资源打包规则
        /// </summary>
        public bool EnableSharePackRule;

        /// <summary>
        /// 是否将单引用资源独立打包
        /// </summary>
        public bool SingleReferencedPackAlone;

        /// <summary>
        /// 加密服务类名称
        /// </summary>
        public string EncryptionServicesClassName;

        /// <summary>
        /// 清单处理服务类名称
        /// </summary>
        public string ManifestProcessServicesClassName;

        /// <summary>
        /// 清单还原服务类名称
        /// </summary>
        public string ManifestRestoreServicesClassName;

        /// <summary>
        /// 文件名称样式
        /// </summary>
        public EFileNameStyle FileNameStyle;
        #endregion

        #region 引擎参数
        /// <summary>
        /// 压缩选项
        /// </summary>
        public ECompressOption CompressOption;

        /// <summary>
        /// 是否禁用写入类型树
        /// </summary>
        public bool DisableWriteTypeTree;

        /// <summary>
        /// 是否忽略类型树变化
        /// </summary>
        public bool IgnoreTypeTreeChanges;

        /// <summary>
        /// 是否使用可寻址地址替换资源路径
        /// </summary>
        public bool ReplaceAssetPathWithAddress;

        /// <summary>
        /// 是否写入 link.xml
        /// </summary>
        public bool WriteLinkXML = true;

        /// <summary>
        /// 缓存服务器地址
        /// </summary>
        public string CacheServerHost;

        /// <summary>
        /// 缓存服务器端口
        /// </summary>
        public int CacheServerPort;

        /// <summary>
        /// 内置 Shader 资源包名称
        /// </summary>
        public string BuiltinShadersBundleName;

        /// <summary>
        /// MonoScript 资源包名称
        /// </summary>
        public string MonoScriptsBundleName;
        #endregion

        #region 构建结果
        /// <summary>
        /// 资源文件总数
        /// </summary>
        public int AssetFileTotalCount;

        /// <summary>
        /// 主资源总数
        /// </summary>
        public int MainAssetTotalCount;

        /// <summary>
        /// 全部资源包总数
        /// </summary>
        public int AllBundleTotalCount;

        /// <summary>
        /// 全部资源包总大小（字节数）
        /// </summary>
        public long AllBundleTotalSize;

        /// <summary>
        /// 加密资源包总数
        /// </summary>
        public int EncryptedBundleTotalCount;

        /// <summary>
        /// 加密资源包总大小（字节数）
        /// </summary>
        public long EncryptedBundleTotalSize;
        #endregion
    }
}
