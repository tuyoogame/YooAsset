using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的清单创建任务
    /// </summary>
    public class TaskCreateManifest_RFBP : TaskCreateManifest, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            CreateManifestFile(false, true, false, context);
        }

        protected override string[] GetBundleDepends(BuildContext context, string bundleName)
        {
            return Array.Empty<string>();
        }
    }
}