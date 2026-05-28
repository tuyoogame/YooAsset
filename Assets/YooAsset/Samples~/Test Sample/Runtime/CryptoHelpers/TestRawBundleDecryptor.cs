using YooAsset;

/// <summary>
/// RawBundle 解密器
/// </summary>
public class TestRawBundleDecryptor : IBundleMemoryDecryptor
{
    byte[] IBundleMemoryDecryptor.GetDecryptedData(BundleDecryptArgs args)
    {
        byte[] data = args.FileData;
        if (data == null)
            data = FileUtility.ReadAllBytes(args.FilePath);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= TestRawBundleEncryptor.KEY;
        }
        return data;
    }
}
