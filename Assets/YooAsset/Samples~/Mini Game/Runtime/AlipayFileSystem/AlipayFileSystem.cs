#if UNITY_WEBGL && UNITY_ALIMINIGAME
using YooAsset;
using AlipaySdk;

public static class AlipayFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService)
    {
        string fileSystemClass = $"{nameof(AlipayFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot, IRemoteService remoteService, IBundleDecryptor decryptor)
    {
        string fileSystemClass = $"{nameof(AlipayFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        fileSystemParams.AddParameter(EFileSystemParameter.RemoteService, remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, decryptor);
        return fileSystemParams;
    }
}

/// <summary>
/// 支付宝小游戏文件系统
/// </summary>
internal class AlipayFileSystem : WebGameFileSystem
{
    protected override IWebGamePlatform CreatePlatform(string packageRoot)
    {
        return new AlipayPlatform();
    }
}
#endif
