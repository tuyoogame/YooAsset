using System.Collections;
using UnityEngine;
using UniFramework.Machine;
using YooAsset;

internal class FsmUpdatePackageManifest : IStateNode
{
    private StateMachine _machine;

    void IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    void IStateNode.OnEnter()
    {
        PatchStepChangedEvent.SendEventMessage("Updating package manifest.");
        GameManager.Instance.StartCoroutine(UpdateManifest());
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }

    private IEnumerator UpdateManifest()
    {
        var packageName = (string)_machine.GetBlackboardValue("PackageName");
        var packageVersion = (string)_machine.GetBlackboardValue("PackageVersion");
        var package = YooAssets.GetPackage(packageName);
        var options = new LoadPackageManifestOptions(packageVersion, 60);
        var operation = package.LoadPackageManifestAsync(options);
        yield return operation;

        if (operation.Status != EOperationStatus.Succeeded)
        {
            Debug.LogWarning(operation.Error);
            PatchPackageManifestUpdateFailedEvent.SendEventMessage();
            yield break;
        }
        else
        {
            _machine.ChangeState<FsmCreateDownloader>();
        }
    }
}