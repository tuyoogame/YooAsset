using YooAsset;

/// <summary>
/// ArchiveBundle 解密器
/// </summary>
public class TestArchiveBundleDecryptor : IBundleMemoryDecryptor
{
    byte[] IBundleMemoryDecryptor.GetDecryptedData(BundleDecryptArgs args)
    {
        byte[] data = args.FileData;
        if (data == null)
            data = FileUtility.ReadAllBytes(args.FilePath);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= TestArchiveBundleEncryptor.KEY;
        }
        return data;
    }
}
