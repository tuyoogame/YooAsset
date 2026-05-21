#if UNITY_WEBGL && OPPOMINIGAME
using YooAsset;

public static class OppoFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        string fileSystemClass = $"{nameof(OppoFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor decryptor)
    {
        string fileSystemClass = $"{nameof(OppoFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, decryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// OPPO 小游戏文件系统
/// </summary>
internal class OppoFileSystem : WebGameFileSystem
{
    /// <inheritdoc/>
    protected override IWebGamePlatform CreatePlatform(string packageRoot)
    {
        return new OppoPlatform();
    }
}
#endif
