#if UNITY_WEBGL && TAPMINIGAME
using YooAsset;
using TapTapMiniGame;

public static class TaptapFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        string fileSystemClass = $"{nameof(TaptapFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor decryptor)
    {
        string fileSystemClass = $"{nameof(TaptapFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, decryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// TapTap小游戏文件系统
/// </summary>
internal class TaptapFileSystem : WebGameFileSystem
{
    protected override IWebGamePlatform CreatePlatform(string packageRoot)
    {
        return new TaptapPlatform();
    }
}
#endif
