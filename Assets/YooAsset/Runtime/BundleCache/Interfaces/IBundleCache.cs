using System;

namespace YooAsset
{
    /// <summary>
    /// 文件缓存系统接口
    /// </summary>
    internal interface IBundleCache : IDisposable
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        string PackageName { get; }

        /// <summary>
        /// 缓存根目录
        /// </summary>
        string RootPath { get; }

        /// <summary>
        /// 是否为只读缓存
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// 缓存文件数量
        /// </summary>
        int FileCount { get; }

        /// <summary>
        /// 已占用空间（字节）
        /// </summary>
        long SpaceOccupied { get; }


        /// <summary>
        /// 初始化缓存系统
        /// </summary>
        /// <returns>初始化异步操作对象</returns>
        BCInitializeOperation InitializeAsync();

        /// <summary>
        /// 将资源包数据写入缓存
        /// </summary>
        /// <param name="options">写入操作的配置参数</param>
        /// <returns>写入缓存异步操作对象</returns>
        BCWriteCacheOperation WriteCacheAsync(BCWriteCacheOptions options);

        /// <summary>
        /// 清理符合淘汰策略的缓存文件
        /// </summary>
        /// <param name="options">清理操作的配置参数</param>
        /// <returns>清理缓存异步操作对象</returns>
        BCClearCacheOperation ClearCacheAsync(BCClearCacheOptions options);

        /// <summary>
        /// 验证缓存文件的完整性
        /// </summary>
        /// <param name="options">验证操作的配置参数</param>
        /// <returns>验证缓存异步操作对象</returns>
        BCVerifyCacheOperation VerifyCacheAsync(BCVerifyCacheOptions options);

        /// <summary>
        /// 加载指定资源包
        /// </summary>
        /// <param name="options">加载操作的配置参数</param>
        /// <returns>加载资源包异步操作对象</returns>
        BCLoadBundleOperation LoadBundleAsync(BCLoadBundleOptions options);

        /// <summary>
        /// 检查指定资源包是否已存在于缓存中
        /// </summary>
        /// <param name="bundleGuid">资源包的唯一标识符</param>
        /// <returns>如果缓存中存在该资源包则返回 true，否则返回 false。</returns>
        bool IsCached(string bundleGuid);

        /// <summary>
        /// 获取已缓存的资源包文件路径
        /// </summary>
        /// <param name="bundleGuid">资源包的唯一标识符</param>
        /// <returns>返回已缓存的资源包文件路径，如果不存在则返回 null。</returns>
        string GetCacheFilePath(string bundleGuid);
    }
}
