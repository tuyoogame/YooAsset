using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 可编程构建管线的构建报告创建任务
    /// </summary>
    public class TaskCreateReport_SBP : TaskCreateReport, IBuildTask
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