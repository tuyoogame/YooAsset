
namespace YooAsset.Editor
{
    /// <summary>
    /// 构建管线类型
    /// </summary>
    public enum EBuildPipeline
    {
        /// <summary>
        /// 编辑器下的模拟构建管线（ESBP）
        /// </summary>
        EditorSimulateBuildPipeline,

        /// <summary>
        /// 旧版构建管线 (LBP)
        /// </summary>
        LegacyBuildPipeline,

        /// <summary>
        /// 可编程构建管线 (SBP)
        /// </summary>
        ScriptableBuildPipeline,

        /// <summary>
        /// 原生文件构建管线 (RFBP)
        /// </summary>
        RawFileBuildPipeline,

        /// <summary>
        /// 团结引擎 InstantAsset 构建管线 (IABP)
        /// </summary>
        InstantAssetBuildPipeline,

        /// <summary>
        /// 归档文件构建管线 (AFBP)
        /// </summary>
        ArchiveFileBuildPipeline,
    }
}