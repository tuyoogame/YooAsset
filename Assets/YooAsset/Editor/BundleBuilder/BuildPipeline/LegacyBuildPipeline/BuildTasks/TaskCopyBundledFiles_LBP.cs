using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的首包资源的拷贝任务
    /// </summary>
    public class TaskCopyBundledFiles_LBP : TaskCopyBundledFiles, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var manifestContext = context.GetContextObject<ManifestContext>();
            if (buildParametersContext.Parameters.BundledCopyOption != EBundledCopyOption.None)
            {
                CopyBundledFilesToStreaming(buildParametersContext, manifestContext.Manifest);
            }
        }
    }
}