namespace YooAsset
{
    /// <summary>
    /// 自定义异步操作基类
    /// </summary>
    public abstract class CustomAsyncOperation : AsyncOperationBase
    {
        /// <summary>
        /// 开始异步操作
        /// </summary>
        public void Start()
        {
            StartOperation();
        }

        /// <summary>
        /// 更新异步操作
        /// </summary>
        public void Update()
        {
            UpdateOperation();
        }

        /// <summary>
        /// 终止异步操作（递归中止所有子任务）
        /// </summary>
        public void Abort()
        {
            AbortOperation();
        }
    }
}
