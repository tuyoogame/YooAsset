using System.Collections;
using UnityEngine;
using Framework.Machine;
using YooAsset;

internal class FsmUpdatePackageManifest : IState<PatchContext>
{
    void IState<PatchContext>.OnEnter(PatchContext context)
    {
        PatchStepChangedEvent.SendEventMessage("Updating package manifest.");
        GameManager.Instance.StartCoroutine(UpdateManifest(context));
    }
    void IState<PatchContext>.OnExit(PatchContext context)
    {
    }

    private IEnumerator UpdateManifest(PatchContext context)
    {
        var packageName = context.PackageName;
        var packageVersion = context.PackageVersion;
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
            context.Machine.RequestChange(PatchState.CreateDownloader);
        }
    }
}