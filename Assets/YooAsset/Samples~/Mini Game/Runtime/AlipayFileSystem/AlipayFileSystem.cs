#if UNITY_WEBGL && UNITY_ALIMINIGAME
using YooAsset;

public static class AlipayFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService)
    {
        var fileSystemParams = CreateBaseFileSystemParameters(remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor)
    {
        var fileSystemParams = CreateBaseFileSystemParameters(remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, assetBundleDecryptor);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor, IBundleDecryptor rawBundleDecryptor)
    {
        var fileSystemParams = CreateBaseFileSystemParameters(remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetbundleDecryptor, assetBundleDecryptor);
        fileSystemParams.AddParameter(EFileSystemParameter.RawbundleDecryptor, rawBundleDecryptor);
        return fileSystemParams;
    }

    private static FileSystemParameters CreateBaseFileSystemParameters(IRemoteService remoteService)
    {
        var fileSystemParams = FileSystemParameters.CreateDefaultWebNetworkFileSystemParameters(remoteService, true);
        fileSystemParams.AddParameter(EFileSystemParameter.WebPlatformStrategy, new AlipayPlatform());
        return fileSystemParams;
    }
}
#endif
