using Framework.Machine;
using YooAsset;

/// <summary>
/// Shared data for the patch workflow.
/// </summary>
internal sealed class PatchContext
{
    public string PackageName { get; }
    public EPlayMode PlayMode { get; }
    public string PackageVersion { get; set; }
    public ResourceDownloaderOperation Downloader { get; set; }
    public FlowMachine<PatchState, PatchContext> Machine { get; set; }

    public PatchContext(string packageName, EPlayMode playMode)
    {
        PackageName = packageName;
        PlayMode = playMode;
    }
}
