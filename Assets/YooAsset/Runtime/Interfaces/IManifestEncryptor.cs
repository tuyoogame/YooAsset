
namespace YooAsset
{
    /// <summary>
    /// 资源清单加密器
    /// </summary>
    public interface IManifestEncryptor
    {
        /// <summary>
        /// 对资源清单的原始数据执行加密
        /// </summary>
        /// <param name="fileData">待加密的清单数据</param>
        /// <returns>加密后的字节数组</returns>
        byte[] Encrypt(byte[] fileData);
    }
}