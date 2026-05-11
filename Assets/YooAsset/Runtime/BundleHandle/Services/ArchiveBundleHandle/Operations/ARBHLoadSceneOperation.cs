
namespace YooAsset
{
    /// <summary>
    /// 场景加载操作（归档资源包不支持）
    /// </summary>
    internal sealed class ARBHLoadSceneOperation : BHLoadSceneOperation
    {
        protected override void InternalStart()
        {
            SetError($"{nameof(ARBHLoadSceneOperation)} does not support scene loading.");
        }
        protected override void InternalUpdate()
        {
        }
        protected override void InternalAllowSceneActivation()
        {
        }
    }
}
