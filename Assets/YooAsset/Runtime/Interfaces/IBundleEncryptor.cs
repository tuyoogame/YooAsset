
namespace YooAsset
{
    /// <summary>
    /// 资源包加密操作的输入参数
    /// </summary>
    public readonly struct BundleEncryptArgs
    {
        /// <summary>
        /// 资源包名称
        /// </summary>
        internal string BundleName { get; }

        /// <summary>
        /// 待加密的源文件路径
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 创建 <see cref="BundleEncryptArgs"/> 实例
        /// </summary>
        /// <param name="bundleName">资源包名称</param>
        /// <param name="filePath">待加密的源文件路径</param>
        internal BundleEncryptArgs(string bundleName, string filePath)
        {
            BundleName = bundleName;
            FilePath = filePath;
        }
    }

    /// <summary>
    /// 资源包加密操作的返回结果
    /// </summary>
    public readonly struct BundleEncryptResult
    {
        /// <summary>
        /// 文件是否已加密
        /// </summary>
        public bool IsEncrypted { get; }

        /// <summary>
        /// 加密后的文件数据
        /// </summary>
        public byte[] EncryptedFileData { get; }

        /// <summary>
        /// 创建 <see cref="BundleEncryptResult"/> 实例
        /// </summary>
        /// <param name="isEncrypted">文件是否已加密</param>
        /// <param name="encryptedFileData">加密后的文件数据，未加密时为 null。</param>
        public BundleEncryptResult(bool isEncrypted, byte[] encryptedFileData)
        {
            IsEncrypted = isEncrypted;
            EncryptedFileData = encryptedFileData;
        }
    }

    /// <summary>
    /// 定义资源包的加密行为
    /// </summary>
    public interface IBundleEncryptor
    {
        /// <summary>
        /// 对指定的资源包文件执行加密
        /// </summary>
        /// <param name="args">加密操作的输入参数</param>
        /// <returns>包含加密状态和加密后数据的结果</returns>
        BundleEncryptResult Encrypt(BundleEncryptArgs args);
    }
}