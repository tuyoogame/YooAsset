using Framework.Machine;

internal class FsmDownloadPackageOver : IState<PatchContext>
{
    void IState<PatchContext>.OnEnter(PatchContext context)
    {
        PatchStepChangedEvent.SendEventMessage("Resource files download completed.");
        context.Machine.RequestChange(PatchState.ClearCacheBundle);
    }
    void IState<PatchContext>.OnExit(PatchContext context)
    {
    }
}