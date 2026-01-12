#if UNITY_WEBGL && DOUYINMINIGAME
using UnityEngine;
using UnityEngine.Networking;
using YooAsset;
using TTSDK;

/// <summary>
/// 抖音小游戏平台实现
/// </summary>
internal class TiktokPlatform : IWebGamePlatform
{
    private readonly TTFileSystemManager _fileSystemMgr;

    /// <summary>
    /// 创建 TiktokPlatform 实例
    /// </summary>
    /// <param name="fileSystemMgr">抖音文件系统管理器</param>
    internal TiktokPlatform(TTFileSystemManager fileSystemMgr)
    {
        _fileSystemMgr = fileSystemMgr;
    }

    /// <inheritdoc/>
    public UnityWebRequest CreateAssetBundleRequest(string url)
    {
        return TTAssetBundle.GetAssetBundle(url);
    }

    /// <inheritdoc/>
    public AssetBundle ExtractAssetBundle(UnityWebRequest request)
    {
        var downloadHandler = (DownloadHandlerTTAssetBundle)request.downloadHandler;
        return downloadHandler.assetBundle;
    }

    /// <inheritdoc/>
    public void UnloadAssetBundle(AssetBundle assetBundle, bool unloadAll)
    {
        assetBundle.TTUnload(unloadAll);
    }

    /// <inheritdoc/>
    public bool IsCached(string cacheFilePath)
    {
        return false;
    }

    /// <inheritdoc/>
    public string GetCacheFilePath(string rootPath, PackageBundle bundle)
    {
        return _fileSystemMgr.GetLocalCachedPathForUrl(bundle.GetFileName());
    }
}
#endif
