namespace YooAsset
{
    /// <summary>
    /// 卸载未使用资源的异步操作
    /// </summary>
    public sealed class UnloadUnusedAssetsOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            UnloadUnused,
            Done,
        }

        private readonly ResourceManager _resourceManager;
        private readonly UnloadUnusedAssetsOptions _options;
        private int _loopCounter = 0;
        private ESteps _steps = ESteps.None;

        internal UnloadUnusedAssetsOperation(ResourceManager resourceManager, UnloadUnusedAssetsOptions options)
        {
            _resourceManager = resourceManager;
            _options = options;
        }
        /// <inheritdoc />
        protected override void InternalStart()
        {
            _steps = ESteps.UnloadUnused;
            _loopCounter = _options.MaxLoopCount;
        }
        /// <inheritdoc />
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.UnloadUnused)
            {
                // 说明：资源包之间会有深层的依赖链表，需要多次迭代才可以在单帧内卸载！
                while (_loopCounter > 0)
                {
                    _loopCounter--;
                    _resourceManager.DestroyUnusedBundle();

                    if (IsBusy)
                        break;
                }

                if (_loopCounter <= 0)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
            }
        }
        /// <inheritdoc />
        protected override void InternalWaitForCompletion()
        {
            ExecuteBatch();
        }
        /// <inheritdoc />
        protected override string InternalGetDescription()
        {
            return $"MaxLoopCount: {_options.MaxLoopCount}";
        }
    }
}