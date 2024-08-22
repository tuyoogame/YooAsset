
namespace YooAsset
{
    public abstract class GameAsyncOperation : AsyncOperationBase
    {
        internal override void InternalOnStart()
        {
            OnStart();
        }
        internal override void InternalOnUpdate()
        {
            OnUpdate();
        }
        internal override void InternalOnAbort()
        {
            OnAbort();
        }
        internal override void InternalWaitForAsyncComplete()
        {
            OnWaitForAsyncComplete();
        }

        /// <summary>
        /// 异步操作开始
        /// </summary>
        protected abstract void OnStart();

        /// <summary>
        /// 异步操作更新
        /// </summary>
        protected abstract void OnUpdate();

        /// <summary>
        /// 异步操作终止
        /// </summary>
        protected abstract void OnAbort();

        /// <summary>
        /// 异步等待完成
        /// </summary>
        protected virtual void OnWaitForAsyncComplete() { }

        /// <summary>
        /// 异步操作系统是否繁忙
        /// </summary>
        protected bool IsBusy()
        {
            return OperationSystem.IsBusy;
        }

        /// <summary>
        /// 终止异步操作
        /// </summary>
        protected void Abort()
        {
            SetAbort();
        }
    }
}