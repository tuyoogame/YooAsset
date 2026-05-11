
namespace YooAsset
{
    /// <summary>
    /// 场景加载操作（虚拟归档资源包不支持）
    /// </summary>
    internal sealed class VARBHLoadSceneOperation : BHLoadSceneOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(VARBHLoadSceneOperation)} does not support scene loading.");
        }
        protected override void InternalUpdate()
        {
        }
        protected override void InternalAllowSceneActivation()
        {
        }
    }
}
