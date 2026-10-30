using Framework.Machine;
using YooAsset;

internal class FsmClearCacheBundle : IState<PatchContext>
{
    private PatchContext _context;

    void IState<PatchContext>.OnEnter(PatchContext context)
    {
        _context = context;
        PatchStepChangedEvent.SendEventMessage("Clearing unused cache files.");
        var packageName = context.PackageName;
        var package = YooAssets.GetPackage(packageName);
        var options = new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles);
        var operation = package.ClearCacheAsync(options);
        operation.Completed += Operation_Completed;
    }
    void IState<PatchContext>.OnExit(PatchContext context)
    {
    }

    private void Operation_Completed(YooAsset.AsyncOperationBase obj)
    {
        _context.Machine.RequestChange(PatchState.StartGame);
    }
}
