internal enum PatchState
{
    InitializePackage,
    RequestPackageVersion,
    UpdatePackageManifest,
    CreateDownloader,
    DownloadPackageFiles,
    DownloadPackageOver,
    ClearCacheBundle,
    StartGame,
}
