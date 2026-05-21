#if UNITY_WEBGL && KUAISHOUMINIGAME
using YooAsset;

public static class KuaiShouFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        string fileSystemClass = $"{nameof(KuaiShouFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor decryptor)
    {
        string fileSystemClass = $"{nameof(KuaiShouFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, decryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// 快手小游戏文件系统
/// </summary>
internal class KuaiShouFileSystem : WebGameFileSystem
{
    /// <inheritdoc/>
    protected override IWebGamePlatform CreatePlatform(string packageRoot)
    {
        return new KuaiShouPlatform();
    }
}
#endif
