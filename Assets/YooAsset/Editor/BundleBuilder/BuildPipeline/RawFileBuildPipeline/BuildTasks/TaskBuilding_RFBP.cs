using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的文件拷贝任务
    /// </summary>
    public class TaskBuilding_RFBP : IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            CopyRawBundle(buildMapContext, buildParametersContext);
        }

        /// <summary>
        /// 拷贝原生文件
        /// </summary>
        private void CopyRawBundle(BuildMapContext buildMapContext, BuildParametersContext buildParametersContext)
        {
            string pipelineOutputDirectory = buildParametersContext.GetPipelineOutputDirectory();
            foreach (var bundleInfo in buildMapContext.Collection)
            {
                string dest = $"{pipelineOutputDirectory}/{bundleInfo.BundleName}";
                foreach (var buildAsset in bundleInfo.AllPackAssets)
                {
                    EditorFileUtility.CopyFile(buildAsset.AssetInfo.AssetPath, dest, true);
                }
            }
        }
    }
}