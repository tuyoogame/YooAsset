#if UNITY_ANDROID && GOOGLE_PLAY
using YooAsset;

public static class GooglePlayFileSystemCreater
{
    public static FileSystemParameters CreateFileSystemParameters(string packageRoot)
    {
        string fileSystemClass = $"{nameof(GooglePlayFileSystem)},YooAsset.MiniGame";
        var fileSystemParams = new FileSystemParameters(fileSystemClass, packageRoot);
        return fileSystemParams;
    }
}

/// <summary>
/// Google Play Asset Delivery file system.
/// Loads asset bundles via PlayAssetDelivery instead of local file I/O.
/// See: https://developer.android.com/guide/playcore/asset-delivery
/// </summary>
internal class GooglePlayFileSystem : BuiltinFileSystem, IFileSystem
{
    /// <summary>
    /// Override bundle loading to use Play Asset Delivery.
    /// Re-implements <see cref="IFileSystem.LoadPackageBundleAsync"/> so the
    /// interface dispatch reaches this method instead of the base class version.
    /// </summary>
    public new FSLoadPackageBundleOperation LoadPackageBundleAsync(FSLoadPackageBundleOptions options)
    {
        var operation = new GPFSLoadPackageBundleOperation(this, options);
        return operation;
    }
}
#endif
