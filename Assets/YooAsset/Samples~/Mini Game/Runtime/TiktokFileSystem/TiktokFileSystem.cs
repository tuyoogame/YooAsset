#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;
using TTSDK;

public static class TiktokFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        string fileSystemClass = $"{nameof(TiktokFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor decryptor)
    {
        string fileSystemClass = $"{nameof(TiktokFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, decryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// 抖音小游戏文件系统
/// </summary>
internal class TiktokFileSystem : WebGameFileSystem
{
    /// <inheritdoc/>
    protected override IWebGamePlatform CreatePlatform(string packageRoot)
    {
        return new TiktokPlatform(TT.GetFileSystemManager());
    }
}
#endif
