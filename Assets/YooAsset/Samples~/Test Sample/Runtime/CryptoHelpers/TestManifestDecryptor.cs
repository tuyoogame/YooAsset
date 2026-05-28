using YooAsset;

/// <summary>
/// 资源清单解密器
/// </summary>
public class TestManifestDecryptor : IManifestDecryptor
{
    byte[] IManifestDecryptor.Decrypt(byte[] fileData)
    {
        return TestXorCrypto.Crypto(fileData, TestManifestEncryptor.KEY);
    }
}
