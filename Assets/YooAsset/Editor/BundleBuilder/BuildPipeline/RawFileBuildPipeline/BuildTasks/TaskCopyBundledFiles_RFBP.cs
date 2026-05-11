using System;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的首包资源的拷贝任务
    /// </summary>
    public class TaskCopyBundledFiles_RFBP : TaskCopyBundledFiles, IBuildTask
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