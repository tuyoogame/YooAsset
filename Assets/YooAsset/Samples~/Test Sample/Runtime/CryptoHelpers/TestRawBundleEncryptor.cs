using System.IO;
using YooAsset;

/// <summary>
/// RawBundle 加密器
/// </summary>
public class TestRawBundleEncryptor : IBundleEncryptor
{
    public const byte KEY = 0x6B;

    public BundleEncryptResult Encrypt(BundleEncryptArgs fileInfo)
    {
        string bundleName = fileInfo.BundleName.ToLowerInvariant();
        if (bundleName.Contains("_testres5_encryptfiles_") == false)
            return new BundleEncryptResult(false, null);

        byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
        for (int i = 0; i < fileData.Length; i++)
        {
            fileData[i] ^= KEY;
        }
        return new BundleEncryptResult(true, fileData);
    }
}
