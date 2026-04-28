using UniFramework.Machine;
using YooAsset;

internal class FsmClearCacheBundle : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        PatchStepChangedEvent.SendEventMessage("Clearing unused cache files.");
        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var package = YooAssets.GetPackage(packageName);
        var options = new ClearCacheOptions(ClearCacheMethods.ClearUnusedBundleFiles);
        var operation = package.ClearCacheAsync(options);
        operation.Completed += Operation_Completed;
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }

    private void Operation_Completed(YooAsset.AsyncOperationBase obj)
    {
        _machine.ChangeState<FsmStartGame>();
    }
}