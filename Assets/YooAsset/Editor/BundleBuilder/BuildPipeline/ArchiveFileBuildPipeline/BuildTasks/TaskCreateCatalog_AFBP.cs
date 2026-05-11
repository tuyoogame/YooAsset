using System;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 归档文件构建管线的资源目录创建任务
    /// </summary>
    public class TaskCreateCatalog_AFBP : TaskCreateCatalog, IBuildTask
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
