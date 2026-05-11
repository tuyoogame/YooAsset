using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的构建报告创建任务
    /// </summary>
    public class TaskCreateReport_AFBP : TaskCreateReport, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParameters = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var manifestContext = context.GetContextObject<ManifestContext>();
            CreateReportFile(buildParameters, buildMapContext, manifestContext);
        }
    }
}
