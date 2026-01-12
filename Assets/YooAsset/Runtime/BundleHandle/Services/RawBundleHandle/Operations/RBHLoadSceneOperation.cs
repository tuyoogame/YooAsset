
namespace YooAsset
{
    /// <summary>
    /// 场景加载操作（原生资源包不支持）
    /// </summary>
    internal sealed class RBHLoadSceneOperation : BHLoadSceneOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(RBHLoadSceneOperation)} does not support scene loading.");
        }
        protected override void InternalUpdate()
        {
        }
        protected override void InternalAllowSceneActivation()
        {
        }
    }
}