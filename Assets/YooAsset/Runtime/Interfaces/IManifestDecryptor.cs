
namespace YooAsset
{
    /// <summary>
    /// 资源清单解密器
    /// </summary>
    public interface IManifestDecryptor
    {
        /// <summary>
        /// 对加密的资源清单数据执行解密
        /// </summary>
        /// <param name="fileData">已加密的清单数据</param>
        /// <returns>解密后的字节数组</returns>
        byte[] Decrypt(byte[] fileData);
    }
}