
namespace YooAsset
{
    /// <summary>
    /// 内置文件访问器接口，提供对 StreamingAssets 目录的直接访问能力。
    /// </summary>
    /// <remarks>
    /// Android 平台，可通过第三方库（如 BetterStreamingAssets）实现该接口，提供同步文件读取能力。</para>
    /// </remarks>
    public interface IBuiltinFileAccessor
    {
        /// <summary>
        /// 检查内置文件是否存在
        /// </summary>
        /// <param name="filePath">内置文件路径</param>
        bool FileExists(string filePath);

        /// <summary>
        /// 读取内置文件的所有字节
        /// </summary>
        /// <param name="filePath">内置文件路径</param>
        byte[] ReadAllBytes(string filePath);
    }
}
