#if UNITY_WEBGL && DOUYINMINIGAME
using YooAsset;

public static class TiktokFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService)
    {
        var fileSystemParams = CreateBaseFileSystemParameters(remoteService);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor)
    {
        var fileSystemParams = CreateBaseFileSystemParameters(remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, assetBundleDecryptor);
        return fileSystemParams;
    }
    public static FileSystemParameters CreateFileSystemParameters(IRemoteService remoteService, IBundleDecryptor assetBundleDecryptor, IBundleDecryptor rawBundleDecryptor)
    {
        var fileSystemParams = CreateBaseFileSystemParameters(remoteService);
        fileSystemParams.AddParameter(EFileSystemParameter.AssetBundleDecryptor, assetBundleDecryptor);
        fileSystemParams.AddParameter(EFileSystemParameter.RawBundleDecryptor, rawBundleDecryptor);
        return fileSystemParams;
    }

    private static FileSystemParameters CreateBaseFileSystemParameters(IRemoteService remoteService)
    {
        var fileSystemParams = FileSystemParameters.CreateDefaultWebNetworkFileSystemParameters(remoteService, true);
        fileSystemParams.AddParameter(EFileSystemParameter.WebPlatformStrategy, new TiktokPlatform());
        return fileSystemParams;
    }
}
#endif
