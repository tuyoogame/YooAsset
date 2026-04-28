using UniFramework.Machine;
using YooAsset;

internal class FsmStartGame : IStateNode
{
    void IStateNode.OnCreate(StateMachine machine)
    {
    }
    void IStateNode.OnEnter()
    {
        PatchStepChangedEvent.SendEventMessage("Starting game.");

        // Set default package.
        GameManager.Instance.SetGamePackage(YooAssets.GetPackage("DefaultPackage"));

        // Change to home scene.
        SceneChangeToHomeEvent.SendEventMessage();
    }
    void IStateNode.OnUpdate()
    {
    }
    void IStateNode.OnExit()
    {
    }
}