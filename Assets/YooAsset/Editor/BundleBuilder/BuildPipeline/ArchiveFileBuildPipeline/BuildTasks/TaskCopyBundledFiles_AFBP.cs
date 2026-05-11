using System;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的首包资源拷贝任务
    /// </summary>
    public class TaskCopyBundledFiles_AFBP : TaskCopyBundledFiles, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            var manifestContext = context.GetContextObject<ManifestContext>();
            if (buildParameters.BundledCopyOption != EBundledCopyOption.None)
            {
                CopyBundledFilesToStreaming(buildParametersContext, manifestContext.Manifest);
            }
        }
    }
}
