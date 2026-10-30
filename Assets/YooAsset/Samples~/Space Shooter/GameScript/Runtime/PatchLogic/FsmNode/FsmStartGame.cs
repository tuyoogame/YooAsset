using Framework.Machine;
using YooAsset;

internal class FsmStartGame : IState<PatchContext>
{
    void IState<PatchContext>.OnEnter(PatchContext context)
    {
        PatchStepChangedEvent.SendEventMessage("Starting game.");

        // Set default package.
        GameManager.Instance.SetGamePackage(YooAssets.GetPackage(context.PackageName));

        // Change to home scene.
        SceneChangeToHomeEvent.SendEventMessage();
    }
    void IState<PatchContext>.OnExit(PatchContext context)
    {
    }
}