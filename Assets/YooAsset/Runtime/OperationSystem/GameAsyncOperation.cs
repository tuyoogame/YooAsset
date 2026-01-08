
namespace YooAsset
{
    public abstract class GameAsyncOperation : AsyncOperationBase
    {
        internal override void InternalStart()
        {
            OnStart();
        }
        internal override void InternalUpdate()
        {
            OnUpdate();
        }

        /// <summary>
        /// 异步操作开始
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// 异步操作更新
        /// </summary>
        protected abstract void OnUpdate();
    }
}