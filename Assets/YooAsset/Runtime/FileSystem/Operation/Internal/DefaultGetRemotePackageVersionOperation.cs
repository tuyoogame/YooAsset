
namespace YooAsset
{
    internal class DefaultGetRemotePackageVersionOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly string _packageName;
        private readonly string _mainURL;
        private readonly string _fallbackURL;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private UnityWebTextRequestOperation _webTextRequestOp;
        private int _requestCount = 0;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 查询的远端版本信息
        /// </summary>
        internal string PackageVersion { set; get; }


        internal DefaultGetRemotePackageVersionOperation(string packageName, string mainURL, string fallbackURL, bool appendTimeTicks, int timeout)
        {
            _packageName = packageName;
            _mainURL = mainURL;
            _fallbackURL = fallbackURL;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }
        internal override void InternalOnStart()
        {
            _requestCount = WebRequestCounter.GetRequestFailedCount(_packageName, nameof(DefaultGetRemotePackageVersionOperation));
            _steps = ESteps.RequestPackageVersion;
        }
        internal override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_webTextRequestOp == null)
                {
                    string url = GetPackageVersionRequestURL();
                    YooLogger.Log($"Beginning to request package version : {url}");
                    _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout);
                    OperationSystem.StartOperation(_packageName, _webTextRequestOp);
                }

                Progress = _webTextRequestOp.Progress;
                if (_webTextRequestOp.IsDone == false)
                    return;

                if (_webTextRequestOp.Status != EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webTextRequestOp.Error;
                    WebRequestCounter.RecordRequestFailed(_packageName, nameof(DefaultGetRemotePackageVersionOperation));
                }
                else
                {
                    PackageVersion = _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Remote package version file content is empty !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                }
            }
        }

        private string GetPackageVersionRequestURL()
        {
            string url;

            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
                url = _mainURL;
            else
                url = _fallbackURL;

            // 在URL末尾添加时间戳
            if (_appendTimeTicks)
                return $"{url}?{System.DateTime.UtcNow.Ticks}";
            else
                return url;
        }
    }
}