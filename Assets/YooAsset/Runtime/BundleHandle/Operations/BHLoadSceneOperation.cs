
namespace YooAsset
{
    /// <summary>
    /// 加载场景操作的抽象基类
    /// </summary>
    internal abstract class BHLoadSceneOperation : AsyncOperationBase
    {
        /// <summary>
        /// 当前加载操作产出的场景对象
        /// </summary>
        public UnityEngine.SceneManagement.Scene Result { get; protected set; }

        /// <summary>
        /// 允许场景在加载完成后进入激活阶段
        /// </summary>
        public void AllowSceneActivation()
        {
            InternalAllowSceneActivation();
        }

        /// <summary>
        /// 执行场景激活的方法
        /// </summary>
        protected abstract void InternalAllowSceneActivation();
    }
}