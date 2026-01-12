
namespace YooAsset.Editor
{
    /// <summary>
    /// 构建任务的标准接口
    /// </summary>
    public interface IBuildTask
    {
        /// <summary>
        /// 执行构建任务
        /// </summary>
        /// <param name="context">构建上下文</param>
        void Run(BuildContext context);
    }
}