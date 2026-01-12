using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的资源目录创建任务
    /// </summary>
    public class TaskCreateCatalog_RFBP : TaskCreateCatalog, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            if (buildParametersContext.Parameters.BundledCopyOption != EBundledCopyOption.None)
            {
                CreateCatalogFile(buildParametersContext);
            }
        }
    }
}