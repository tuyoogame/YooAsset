
namespace YooAsset.Editor
{
    public interface IBuildPipeline
    {
        BuildResult Run(BuildParameters buildParameters, bool enableLog);
    }
}