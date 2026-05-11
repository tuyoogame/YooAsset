
namespace YooAsset.Editor
{
    /// <summary>
    /// 编辑器模拟构建管线的准备任务
    /// </summary>
    public class TaskPrepare_ESBP : TaskPrepare, IBuildTask
    {
        /// <inheritdoc/>
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            
            // 检测构建参数
            buildParametersContext.CheckBuildParameters();
        }
    }
}
