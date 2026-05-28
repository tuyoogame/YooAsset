using System.IO;
using YooAsset;

/// <summary>
/// AssetBundle 加密器
/// </summary>
public class TestAssetBundleEncryptor : IBundleEncryptor
{
    public const byte KEY = 0x5A;

    public BundleEncryptResult Encrypt(BundleEncryptArgs fileInfo)
    {
        string bundleName = fileInfo.BundleName.ToLowerInvariant();
        if (bundleName.Contains("_testres_encryptfiles_") == false &&
            bundleName.Contains("_testres3_importfiles_") == false &&
            bundleName.Contains("_testres3_unpackfiles_") == false)
            return new BundleEncryptResult(false, null);

        byte[] fileData = File.ReadAllBytes(fileInfo.FilePath);
        for (int i = 0; i < fileData.Length; i++)
        {
            fileData[i] ^= KEY;
        }
        return new BundleEncryptResult(true, fileData);
    }
}
