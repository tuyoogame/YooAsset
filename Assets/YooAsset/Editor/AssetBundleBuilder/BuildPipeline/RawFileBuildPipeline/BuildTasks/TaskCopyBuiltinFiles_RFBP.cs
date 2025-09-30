using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskCopyBuiltinFiles_RFBP : TaskCopyBuiltinFiles, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters;
            var manifestContext = context.GetContextObject<ManifestContext>();
            if (buildParameters.BuiltinFileCopyOption != EBuiltinFileCopyOption.None)
            {
                CopyBuiltinFilesToStreaming(buildParametersContext, manifestContext.Manifest);
            }
        }
    }
}