using System;
using System.Text;
using System.Collections;
using YooAsset;

/// <summary>
/// 测试用清单加密器，使用 XOR 方式加密清单数据
/// </summary>
public class TestManifestEncryptor : IManifestEncryptor
{
    byte[] IManifestEncryptor.Encrypt(byte[] fileData)
    {
        return TestXorCrypto.Crypto(fileData, "YOO");
    }
}

/// <summary>
/// 测试用清单解密器，使用 XOR 方式解密清单数据
/// </summary>
public class TestManifestDecryptor : IManifestDecryptor
{
    byte[] IManifestDecryptor.Decrypt(byte[] fileData)
    {
        return TestXorCrypto.Crypto(fileData, "YOO");
    }
}

/// <summary>
/// XOR 加密/解密工具类（对称算法，加密与解密使用同一方法）
/// </summary>
public class TestXorCrypto
{
    /// <summary>
    /// 使用异或加密/解密字节数组
    /// </summary>
    /// <param name="data">输入数据</param>
    /// <param name="key">加密密钥</param>
    /// <returns>处理后的字节数组</returns>
    public static byte[] Crypto(byte[] data, byte[] key)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        if (key == null || key.Length == 0)
            throw new ArgumentException("Key cannot be null or empty");

        byte[] result = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            // 循环使用密钥中的字节
            result[i] = (byte)(data[i] ^ key[i % key.Length]);
        }

        return result;
    }

    /// <summary>
    /// 使用字符串密钥进行异或处理（自动转换编码）
    /// </summary>
    /// <param name="data">输入数据</param>
    /// <param name="key">字符串密钥</param>
    /// <returns>处理后的字节数组</returns>
    public static byte[] Crypto(byte[] data, string key)
    {
        byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        return Crypto(data, keyBytes);
    }
}