#if TUANJIE_1_8_OR_NEWER && YOOASSET_INSTANT_ASSET_SUPPORT
namespace YooAsset.Editor
{
    /// <summary>
    /// 团结构建管线的准备任务
    /// </summary>
    public class TaskPrepare_IABP : TaskPrepare, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as InstantAssetBuildParameters;

            // 检测构建参数
            buildParametersContext.CheckBuildParameters();

            // 检测未保存场景
            CheckDirtyScenes();

            // 删除历史缓存
            if (buildParameters.ClearBuildCacheFiles)
            {
                DeletePackageRootDirectory(buildParameters);
            }

            // 准备输出目录
            PrepareOutputDirectory(buildParameters);
        }
    }
}
#endif
