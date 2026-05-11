
namespace YooAsset.Editor
{
    /// <summary>
    /// 原生文件构建管线的构建参数
    /// </summary>
    public class RawFileBuildParameters : BuildParameters
    {
        /// <summary>
        /// 文件哈希值计算包含路径信息
        /// </summary>
        public bool IncludePathInHash = false;
    }
}