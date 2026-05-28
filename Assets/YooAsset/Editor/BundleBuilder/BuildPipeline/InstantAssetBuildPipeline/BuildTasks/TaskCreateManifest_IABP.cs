#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
using System;

namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的清单创建任务
    /// </summary>
    public class TaskCreateManifest_IABP : TaskCreateManifest, IBuildTask
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
#endif
