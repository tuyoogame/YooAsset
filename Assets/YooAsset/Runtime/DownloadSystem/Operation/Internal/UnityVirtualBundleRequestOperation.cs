using UnityEngine.Networking;
using UnityEngine;

namespace YooAsset
{
    internal class UnityVirtualBundleRequestOperation : UnityWebRequestOperation
    {
        protected enum ESteps
        {
            None,
            Download,
            Done,
        }

        private readonly PackageBundle _bundle;
        private readonly int _downloadSpeed;
        private ESteps _steps = ESteps.None;

        internal UnityVirtualBundleRequestOperation(PackageBundle packageBundle, int downloadSpeed, string url) : base(url)
        {
            _bundle = packageBundle;
            _downloadSpeed = downloadSpeed;
        }
        internal override void InternalStart()
        {
            _steps = ESteps.Download;
        }
        internal override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.Download)
            {
                // 模拟下载进度
                float progress = 0;
                if (DownloadedBytes > 0)
                    progress = DownloadedBytes / _bundle.FileSize;
                long downloadBytes = (long)((double)_downloadSpeed * Time.deltaTime);
                
                Progress = progress;
                DownloadProgress = progress;
                DownloadedBytes += downloadBytes;
                if (DownloadedBytes < _bundle.FileSize)
                    return;

                Progress = 1f;
                DownloadProgress = 1f;
                DownloadedBytes = _bundle.FileSize;

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
        internal override void InternalWaitForAsyncComplete()
        {
            if (_steps != ESteps.Done)
            {
                _steps = ESteps.Done;
                Status = EOperationStatus.Failed;
                Error = $"Try load bundle {_bundle.BundleName} from remote !";
            }
        }
    }
}