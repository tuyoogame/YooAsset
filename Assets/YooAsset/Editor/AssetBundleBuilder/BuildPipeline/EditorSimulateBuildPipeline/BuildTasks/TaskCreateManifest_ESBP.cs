
using System;

namespace YooAsset.Editor
{
    public class TaskCreateManifest_ESBP : TaskCreateManifest, IBuildTask
    {
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