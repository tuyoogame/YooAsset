
namespace YooAsset
{
    public interface IWechatQueryServices
    {
        /// <summary>
        /// 查询是否为微信缓存的资源文件
        /// </summary>
        /// <param name="packageName">包裹名称</param>
        /// <param name="fileName">文件名称（包含文件的后缀格式）</param>
        /// <param name="fileCRC">文件哈希值</param>
        /// <returns>返回查询结果</returns>
        bool Query(string packageName, string fileName, string fileCRC);
    }
}