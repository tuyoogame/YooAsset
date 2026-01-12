#if UNITY_WEBGL && WEIXINMINIGAME
using YooAsset;
using WeChatWASM;

public static class WechatFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        string fileSystemClass = $"{nameof(WechatFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor decryptor)
    {
        string fileSystemClass = $"{nameof(WechatFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, decryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// 微信小游戏文件系统
/// </summary>
internal class WechatFileSystem : WebGameFileSystem
{
    private string _wxCacheRoot;
    private WechatPlatform _wechatPlatform;

    /// <inheritdoc />
    public override FSClearCacheOperation ClearCacheAsync(FSClearCacheOptions options)
    {
        if (options.ClearMethod == ClearCacheMethods.ClearAllBundleFiles)
        {
            var operation = new WXFSClearAllBundleFilesOperation(this);
            return operation;
        }
        else if (options.ClearMethod == ClearCacheMethods.ClearUnusedBundleFiles)
        {
            var operation = new WXFSClearUnusedBundleFilesAsync(this, options.Manifest);
            return operation;
        }
        else
        {
            string error = $"Invalid clear method : {options.ClearMethod}";
            var operation = new FSClearCacheCompleteOperation(error);
            return operation;
        }
    }

    /// <inheritdoc />
    protected override IWebGamePlatform CreatePlatform(string packageRoot)
    {
        _wxCacheRoot = packageRoot;
        _wechatPlatform = new WechatPlatform();
        return _wechatPlatform;
    }

    internal string GetWXCacheRoot()
    {
        return _wxCacheRoot;
    }
}
#endif