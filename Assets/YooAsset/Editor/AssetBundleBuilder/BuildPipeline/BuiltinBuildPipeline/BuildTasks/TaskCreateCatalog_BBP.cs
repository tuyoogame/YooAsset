using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskCreateCatalog_BBP : TaskCreateCatalog, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            if (buildParametersContext.Parameters.BuiltinFileCopyOption != EBuiltinFileCopyOption.None)
            {
                CreateCatalogFile(buildParametersContext);
            }
        }
    }
}