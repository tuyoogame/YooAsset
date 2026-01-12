using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
    /// <summary>
    /// 旧版构建管线的构建映射生成任务
    /// </summary>
    public class TaskGetBuildMap_LBP : TaskGetBuildMap, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = CreateBuildMap(false, buildParametersContext.Parameters);
            context.SetContextObject(buildMapContext);
        }
    }
}