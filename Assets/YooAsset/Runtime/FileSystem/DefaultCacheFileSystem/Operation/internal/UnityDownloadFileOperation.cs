
namespace YooAsset
{
    internal abstract class UnityDownloadFileOperation : UnityWebRequestOperation
    {
        protected enum ESteps
        {
            None,
            CreateRequest,
            Download,
            CopyLocalFile,
            VerifyFile,
            Done,
        }

        protected readonly DefaultCacheFileSystem _fileSystem;
        protected readonly PackageBundle _bundle;
        protected readonly string _tempFilePath;

        private bool _watchDogInit = false;
        private bool _watchDogAborted = false;
        private ulong _lastDownloadBytes;
        private double _lastGetDataTime;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; }

        internal UnityDownloadFileOperation(DefaultCacheFileSystem fileSystem, PackageBundle bundle, string url) : base(url)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
            _tempFilePath = _fileSystem.GetTempFilePath(bundle);
        }
        internal override string InternalGetDesc()
        {
            return $"RefCount : {RefCount}";
        }

        /// <summary>
        /// 更新看门狗监测
        /// 说明：监控时间范围内，如果没有接收到任何下载数据，那么直接终止任务！
        /// </summary>
        protected void UpdateWatchDog()
        {
            if (_fileSystem.DownloadWatchDogTime == int.MaxValue)
                return;

            if (_watchDogAborted)
                return;

#if UNITY_2020_3_OR_NEWER
            double realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartupAsDouble;
#else
            double realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
#endif

            if (_watchDogInit == false)
            {
                _watchDogInit = true;
                _lastDownloadBytes = 0;
                _lastGetDataTime = realtimeSinceStartup;
            }

            if (_webRequest.downloadedBytes != _lastDownloadBytes)
            {
                _lastDownloadBytes = _webRequest.downloadedBytes;
                _lastGetDataTime = realtimeSinceStartup;
            }
            else
            {
                double deltaTime = realtimeSinceStartup - _lastGetDataTime;
                if (deltaTime > _fileSystem.DownloadWatchDogTime)
                {
                    _watchDogAborted = true;
                    InternalAbort(); //终止网络请求
                }
            }
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public void Reference()
        {
            RefCount++;
        }
    }
}