
namespace YooAsset.Editor
{
    public class ScannerResult
    {
        /// <summary>
        /// 生成的报告文件路径
        /// </summary>
        public string ReprotFilePath { private set; get; }

        /// <summary>
        /// 报告对象
        /// </summary>
        public ScanReport Report { private set; get; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorInfo { private set; get; }

        /// <summary>
        /// 错误堆栈
        /// </summary>
        public string ErrorStack { private set; get; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Succeed
        {
            get
            {
                if (string.IsNullOrEmpty(ErrorInfo))
                    return true;
                else
                    return false;
            }
        }


        public ScannerResult(string error, string stack)
        {
            ErrorInfo = error;
            ErrorStack = stack;
        }
        public ScannerResult(string filePath, ScanReport report)
        {
            ReprotFilePath = filePath;
            Report = report;
            ErrorInfo = string.Empty;
        }

        /// <summary>
        /// 打开报告窗口
        /// </summary>
        public void OpenReportWindow()
        {
            if (Succeed)
            {
                var reproterWindow = AssetArtReporterWindow.OpenWindow();
                reproterWindow.ImportSingleReprotFile(Report);
            }
        }
    }
}