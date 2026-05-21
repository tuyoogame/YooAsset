#if UNITY_WEBGL && VIVOMINIGAME
using YooAsset;

public static class VivoFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        string fileSystemClass = $"{nameof(VivoFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor decryptor)
    {
        string fileSystemClass = $"{nameof(VivoFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, decryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// vivo 小游戏文件系统
/// </summary>
internal class VivoFileSystem : WebGameFileSystem
{
    /// <inheritdoc/>
    protected override IWebGamePlatform CreatePlatform(string packageRoot)
    {
        return new VivoPlatform();
    }
}
#endif
