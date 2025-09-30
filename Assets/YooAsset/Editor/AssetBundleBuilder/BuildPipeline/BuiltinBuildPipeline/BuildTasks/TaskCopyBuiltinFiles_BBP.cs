using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskCopyBuiltinFiles_BBP : TaskCopyBuiltinFiles, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var manifestContext = context.GetContextObject<ManifestContext>();
            if (buildParametersContext.Parameters.BuiltinFileCopyOption != EBuiltinFileCopyOption.None)
            {
                CopyBuiltinFilesToStreaming(buildParametersContext, manifestContext.Manifest);
            }
        }
    }
}