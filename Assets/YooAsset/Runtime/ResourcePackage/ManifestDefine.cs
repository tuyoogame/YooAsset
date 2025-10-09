
namespace YooAsset
{
    public class ManifestDefine
    {
        /// <summary>
        /// 文件极限大小（100MB）
        /// </summary>
        public const int FileMaxSize = 104857600;

        /// <summary>
        /// 文件头标记
        /// </summary>
        public const uint FileSign = 0x594F4F;

        /// <summary>
        /// 文件格式版本
        /// </summary>
        public const string FileVersion = "2025.9.30";
        public const string VERSION_2025_8_28 = "2025.8.28";
        public const string VERSION_2025_9_30 = "2025.9.30";

        /// <summary>
        /// 版本兼容
        /// </summary>
        public const bool BackwardCompatible = true;
    }
}