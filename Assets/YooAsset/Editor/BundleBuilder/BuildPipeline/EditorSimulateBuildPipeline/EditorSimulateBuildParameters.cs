using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器模拟构建管线的构建参数
    /// </summary>
    public class EditorSimulateBuildParameters : BuildParameters
    {
        /// <inheritdoc />
        protected override void CheckBuildParametersCore()
        {
            // EditorSimulateBuildPipeline 只允许 VirtualBundle 类型
            if (BuildBundleType != (int)EBundleType.VirtualAssetBundle &&
                BuildBundleType != (int)EBundleType.VirtualRawBundle &&
                BuildBundleType != (int)EBundleType.VirtualArchiveBundle)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.BuildBundleTypeNotSupported,
                    $"{nameof(EditorSimulateBuildPipeline)} only supports VirtualBundle types. Received: {(EBundleType)BuildBundleType}.");
                throw new InvalidOperationException(message);
            }
        }
    }
}