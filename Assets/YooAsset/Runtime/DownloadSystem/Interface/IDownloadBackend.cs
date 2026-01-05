using System;

namespace YooAsset
{
    /// <summary>
    /// 下载后台接口
    /// </summary>
    /// <remarks>
    /// 不同网络库（UnityWebRequest / BestHTTP / 自研）实现该接口，用于创建具体下载请求。
    /// 每个后台实例是独立的，不共享全局状态。
    /// </remarks>
    internal interface IDownloadBackend : IDisposable
    {
        /// <summary>
        /// 后台名称（用于日志与调试）
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 驱动更新
        /// </summary>
        /// <remarks>
        /// 部分第三方网络库需要在 Unity 主线程中周期性调用 Update 进行驱动。
        /// 不需要驱动的后台可实现为空方法。
        /// </remarks>
        void Update();

        /// <summary>
        /// 创建 HEAD 请求
        /// </summary>
        /// <remarks>
        /// 仅获取响应头信息，不下载实际内容。
        /// 用于检查资源是否存在、获取资源大小、检查缓存有效性等场景。
        /// </remarks>
        /// <param name="args">数据请求参数</param>
        /// <returns>HEAD 请求实例</returns>
        IDownloadHeadRequest CreateHeadRequest(DownloadDataRequestArgs args);

        /// <summary>
        /// 创建文件下载请求
        /// </summary>
        /// <param name="args">文件下载参数</param>
        /// <returns>文件下载请求实例</returns>
        IDownloadFileRequest CreateFileRequest(DownloadFileRequestArgs args);

        /// <summary>
        /// 创建内存下载请求（字节数组）
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <returns>字节下载请求实例</returns>
        IDownloadBytesRequest CreateBytesRequest(DownloadDataRequestArgs args);

        /// <summary>
        /// 创建文本下载请求
        /// </summary>
        /// <param name="args">数据下载参数</param>
        /// <returns>文本下载请求实例</returns>
        IDownloadTextRequest CreateTextRequest(DownloadDataRequestArgs args);

        /// <summary>
        /// 创建 AssetBundle 下载请求
        /// </summary>
        /// <param name="args">AssetBundle 下载参数</param>
        /// <returns>AssetBundle 下载请求实例</returns>
        IDownloadAssetBundleRequest CreateAssetBundleRequest(DownloadAssetBundleRequestArgs args);

        /// <summary>
        /// 创建模拟下载请求
        /// </summary>
        /// <remarks>
        /// 用于编辑器模式下模拟下载进度，不进行实际网络请求。
        /// 可用于测试下载流程和 UI 展示。
        /// </remarks>
        /// <param name="args">模拟下载参数</param>
        /// <returns>模拟下载请求实例</returns>
        IDownloadFileRequest CreateSimulateRequest(DownloadSimulateRequestArgs args);
    }
}
