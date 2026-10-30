using System.Collections;
using UnityEngine;
using Framework.Machine;
using YooAsset;

internal class FsmRequestPackageVersion : IState<PatchContext>
{
    void IState<PatchContext>.OnEnter(PatchContext context)
    {
        PatchStepChangedEvent.SendEventMessage("Requesting package version.");
        GameManager.Instance.StartCoroutine(UpdatePackageVersion(context));
    }
    void IState<PatchContext>.OnExit(PatchContext context)
    {
    }

    private IEnumerator UpdatePackageVersion(PatchContext context)
    {
        var packageName = context.PackageName;
        var package = YooAssets.GetPackage(packageName);
        var operation = package.RequestPackageVersionAsync();
        yield return operation;

        if (operation.Status != EOperationStatus.Succeeded)
        {
            Debug.LogWarning(operation.Error);
            PatchPackageVersionRequestFailedEvent.SendEventMessage();
        }
        else
        {
            Debug.Log($"Package version requested: '{operation.PackageVersion}'.");
            context.PackageVersion = operation.PackageVersion;
            context.Machine.RequestChange(PatchState.UpdatePackageManifest);
        }
    }
}