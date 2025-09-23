
namespace YooAsset.Editor
{
    public class ScannerResult
    {
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
        public ScannerResult(ScanReport report)
        {
            Report = report;
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

        /// <summary>
        /// 保存报告文件
        /// </summary>
        public void SaveReportFile(string saveDirectory)
        {
            if (Report == null)
                throw new System.Exception("Scan report is invalid !");

            if (string.IsNullOrEmpty(saveDirectory))
                saveDirectory = "Assets/";
            string filePath = $"{saveDirectory}/{Report.ReportName}_{Report.ReportDesc}.json";
            ScanReportConfig.ExportJsonConfig(filePath, Report);
        }
    }
}