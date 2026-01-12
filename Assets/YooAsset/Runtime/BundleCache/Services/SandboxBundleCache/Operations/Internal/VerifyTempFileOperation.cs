using System;
using System.Threading;

namespace YooAsset
{
    /// <summary>
    /// 下载文件验证（线程版）
    /// </summary>
    internal sealed class VerifyTempFileOperation : AsyncOperationBase
    {
        private enum ESteps
        {
            None,
            VerifyFile,
            Waiting,
            Done,
        }

        private readonly TempFileInfo _element;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 验证结果
        /// </summary>
        public EFileVerifyResult VerifyResult { private set; get; }

        /// <summary>
        /// 创建下载文件验证（线程版）实例
        /// </summary>
        /// <param name="element">临时文件信息</param>
        internal VerifyTempFileOperation(TempFileInfo element)
        {
            _element = element;
        }
        protected override void InternalStart()
        {
            _steps = ESteps.VerifyFile;
        }
        protected override void InternalUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
                return;

            if (_steps == ESteps.VerifyFile)
            {
                bool succeed = ThreadPool.QueueUserWorkItem(new WaitCallback(VerifyFileInThread), _element);
                if (succeed == false)
                    VerifyFileInThread(_element);

                _steps = ESteps.Waiting;
            }

            if (_steps == ESteps.Waiting)
            {
                int resultCode = _element.VerifyResultCode; //注意: 一次命令取值
                if (resultCode == 0)
                    return;

                VerifyResult = (EFileVerifyResult)resultCode;
                if (VerifyResult == EFileVerifyResult.Succeed)
                {
                    _steps = ESteps.Done;
                    SetResult();
                }
                else
                {
                    _steps = ESteps.Done;
                    SetError($"Failed to verify file '{_element.FilePath}'. ErrorCode: {VerifyResult}.");
                }
            }
        }
        protected override void InternalWaitForCompletion()
        {
            //注意: 等待子线程验证文件完毕，该操作会挂起主线程！
            ExecuteUntilComplete();
        }

        private void VerifyFileInThread(object obj)
        {
            TempFileInfo element = (TempFileInfo)obj;
            int resultCode = (int)FileVerifyHelper.VerifyFile(element.FilePath, element.FileSize, element.FileCrc);
            element.VerifyResultCode = resultCode; //注意: 一次命令赋值
        }
    }
}