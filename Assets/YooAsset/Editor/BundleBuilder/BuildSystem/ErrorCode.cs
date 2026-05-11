
namespace YooAsset.Editor
{
    /// <summary>
    /// 构建流程的错误码定义
    /// </summary>
    internal enum ErrorCode
    {
        // TaskPrepare

        /// <summary>
        /// 构建管线正在运行中
        /// </summary>
        ThePipelineIsBuiding = 100,

        /// <summary>
        /// 发现未保存的场景
        /// </summary>
        FoundUnsavedScene = 101,

        /// <summary>
        /// 未指定构建目标平台
        /// </summary>
        NoBuildTarget = 110,

        /// <summary>
        /// 包裹名称为空
        /// </summary>
        PackageNameIsNullOrEmpty = 111,

        /// <summary>
        /// 包裹版本号为空
        /// </summary>
        PackageVersionIsNullOrEmpty = 112,

        /// <summary>
        /// 构建输出根目录为空
        /// </summary>
        BuildOutputRootIsNullOrEmpty = 113,

        /// <summary>
        /// 首包资源根目录为空
        /// </summary>
        BundledRootIsNullOrEmpty = 114,

        /// <summary>
        /// 构建输出目录已存在
        /// </summary>
        PackageOutputDirectoryExists = 115,

        /// <summary>
        /// 构建管线名称为空
        /// </summary>
        BuildPipelineIsNullOrEmpty = 116,

        /// <summary>
        /// 未知的资源包构建类型
        /// </summary>
        BuildBundleTypeIsUnknown = 117,

        /// <summary>
        /// 构建管线不支持指定的资源包类型
        /// </summary>
        BuildBundleTypeNotSupported = 118,

        /// <summary>
        /// 构建管线不支持资源包加密
        /// </summary>
        BundleEncryptionNotSupported = 119,

        /// <summary>
        /// 建议使用 SBP 构建管线
        /// </summary>
        RecommendScriptBuildPipeline = 130,

        /// <summary>
        /// 内置着色器资源包名称为空
        /// </summary>
        BuiltinShadersBundleNameIsNull = 131,

        // TaskGetBuildMap

        /// <summary>
        /// 移除了无效的资源标签
        /// </summary>
        RemoveInvalidTags = 200,

        /// <summary>
        /// 发现未被依赖的资源
        /// </summary>
        FoundUndependedAsset = 201,

        /// <summary>
        /// 打包资源列表为空
        /// </summary>
        PackAssetListIsEmpty = 202,

        /// <summary>
        /// 不支持多个原生资源打入同一资源包
        /// </summary>
        NotSupportMultipleRawAsset = 210,

        // TaskBuilding

        /// <summary>
        /// Unity 引擎构建失败
        /// </summary>
        UnityEngineBuildFailed = 300,

        /// <summary>
        /// Unity 引擎构建发生致命错误
        /// </summary>
        UnityEngineBuildFatal = 301,

        // TaskUpdateBundleInfo

        /// <summary>
        /// 资源包名称字符数超出限制
        /// </summary>
        CharactersOverTheLimit = 400,

        /// <summary>
        /// 未找到 Unity 资源包哈希值
        /// </summary>
        NotFoundUnityBundleHash = 401,

        /// <summary>
        /// 未找到 Unity 资源包 CRC 值
        /// </summary>
        NotFoundUnityBundleCRC = 402,

        /// <summary>
        /// 资源包临时文件大小为零
        /// </summary>
        BundleTempSizeIsZero = 403,

        /// <summary>
        /// 未找到 InstantAsset 资源包 GUID 映射
        /// </summary>
        NotFoundInstantAssetBundleGuid = 404,

        // TaskVerifyBuildResult

        /// <summary>
        /// 发现非预期的构建产物
        /// </summary>
        UnintendedBuildBundle = 500,

        /// <summary>
        /// 构建结果中存在非预期内容
        /// </summary>
        UnintendedBuildResult = 501,

        /// <summary>
        /// 构建结果中缺失预期的资源包
        /// </summary>
        MissingExpectedBundle = 502,

        /// <summary>
        /// InstantAsset 构建日志文件无效
        /// </summary>
        InvalidInstantAssetBuildLog = 503,

        // TaskCreateManifest

        /// <summary>
        /// 构建结果中未找到对应的 Unity 资源包
        /// </summary>
        NotFoundUnityBundleInBuildResult = 600,

        /// <summary>
        /// 发现游离的资源包（不在构建映射中）
        /// </summary>
        FoundStrayBundle = 601,

        /// <summary>
        /// 资源包哈希值冲突
        /// </summary>
        BundleHashConflict = 602,

        // TaskCopyBundledFiles

        /// <summary>
        /// 首包资源的拷贝参数为空
        /// </summary>
        BundledCopyParamsIsNullOrEmpty = 700,
    }
}