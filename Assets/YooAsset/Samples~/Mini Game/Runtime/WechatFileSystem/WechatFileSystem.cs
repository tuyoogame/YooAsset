#if UNITY_WEBGL && (WEIXINMINIGAME || UNITY_WECHATMINIGAME)
using YooAsset;
using WeChatWASM;

public static class WechatFileSystemCreater
{
    private static string DefaultWXCacheRoot => $"{WX.env.USER_DATA_PATH}/__GAME_FILE_CACHE";

    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService)
    {
        var fileSystemParams = CreateFileSystemParameters(DefaultWXCacheRoot, remoteService, null, null);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor)
    {
        var fileSystemParams = CreateFileSystemParameters(DefaultWXCacheRoot, remoteService, assetBundleDecryptor, null);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor, IBundleDecryptor rawBundleDecryptor)
    {
        var fileSystemParams = CreateFileSystemParameters(DefaultWXCacheRoot, remoteService, assetBundleDecryptor, rawBundleDecryptor);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        var fileSystemParams = CreateFileSystemParameters(packageRoot, remoteService, null, null);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor)
    {
        var fileSystemParams = CreateFileSystemParameters(packageRoot, remoteService, assetBundleDecryptor, null);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor, IBundleDecryptor rawBundleDecryptor)
    {
        string fileSystemClass = $"{nameof(WechatFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.DisableUnityWebCache, true);
        fileSystemParams.AddParameter(EFileSystemParameter.WebPlatformStrategy, new WechatPlatform());

        if (assetBundleDecryptor != null)
            fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, assetBundleDecryptor);
        if (rawBundleDecryptor != null)
            fileSystemParams.AddParameter(EFileSystemParameter.RawbundleDecryptor, rawBundleDecryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// 微信小游戏文件系统
/// </summary>
internal class WechatFileSystem : WebNetworkFileSystem
{
    private string _wxCacheRoot;

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
    public override void OnCreate(string packageName, string packageRoot)
    {
        _wxCacheRoot = packageRoot;
        base.OnCreate(packageName, packageRoot);
    }

    internal string GetWXCacheRoot()
    {
        return _wxCacheRoot;
    }
}
#endif