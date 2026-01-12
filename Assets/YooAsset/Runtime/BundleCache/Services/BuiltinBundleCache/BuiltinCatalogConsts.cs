
namespace YooAsset
{
    /// <summary>
    /// 内置资源目录常量定义
    /// </summary>
    internal static class BuiltinCatalogConsts
    {
        /// <summary>
        /// 文件极限大小（100MB）
        /// </summary>
        public const int MaxFileSize = 104857600;

        /// <summary>
        /// 文件头标识
        /// </summary>
        public const uint FileMagic = 0x133C5EE;

        /// <summary>
        /// 文件版本号
        /// </summary>
        public const int FileVersion = 1;


        /// <summary>
        /// JSON文件名称
        /// </summary>
        public const string JsonFileName = "BuiltinCatalog.json";

        /// <summary>
        /// 二进制文件名称
        /// </summary>
        public const string BinaryFileName = "BuiltinCatalog.bytes";
    }
}