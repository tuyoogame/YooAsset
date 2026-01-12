using System.Collections.Generic;

namespace YooAsset
{
    /// <summary>
    /// 远端资源服务
    /// </summary>
    public interface IRemoteService
    {
        /// <summary>
        /// 获取指定文件的所有远端候选地址，按优先级排序。
        /// </summary>
        /// <param name="fileName">请求的文件名称</param>
        /// <returns>按优先级排序的远端候选地址列表，至少包含一个 URL。</returns>
        IReadOnlyList<string> GetRemoteUrls(string fileName);
    }
}
