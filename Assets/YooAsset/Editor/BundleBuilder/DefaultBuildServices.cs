
namespace YooAsset.Editor
{
    /// <summary>
    /// 空加密实现，不对资源包进行任何加密处理
    /// </summary>
    public class EncryptionNone : IBundleEncryptor
    {
        /// <inheritdoc/>
        public BundleEncryptResult Encrypt(BundleEncryptArgs fileInfo)
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 空清单加密实现，不对资源清单进行任何加密处理
    /// </summary>
    public class ManifestEncryptorNone : IManifestEncryptor
    {
        /// <inheritdoc/>
        byte[] IManifestEncryptor.Encrypt(byte[] fileData)
        {
            return fileData;
        }
    }

    /// <summary>
    /// 空清单解密实现，不对资源清单进行任何解密处理
    /// </summary>
    public class ManifestDecryptorNone : IManifestDecryptor
    {
        /// <inheritdoc/>
        byte[] IManifestDecryptor.Decrypt(byte[] fileData)
        {
            return fileData;
        }
    }
}
