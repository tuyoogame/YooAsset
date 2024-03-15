
namespace YooAsset
{
    public interface IRemoteServices
    {
        /// <summary>
        /// 获取主资源站的资源地址
        /// <param name="packageName">请求的文件所在package名称</param>
        /// <param name="fileName">请求的文件名称</param>
        /// </summary>
        string GetRemoteMainURL(string packageName, string fileName);

        /// <summary>
        /// 获取备用资源站的资源地址
        /// </summary>
        /// <param name="packageName">请求的文件所在package名称</param>
        /// <param name="fileName">请求的文件名称</param>
        string GetRemoteFallbackURL(string packageName, string fileName);
    }
}