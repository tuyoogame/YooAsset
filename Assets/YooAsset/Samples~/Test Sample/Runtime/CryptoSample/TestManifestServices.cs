using System;
using System.IO;
using System.Text;
using System.Collections;
using UnityEngine;
using NUnit.Framework;
using YooAsset;

public class TestProcessManifest : IManifestProcessServices
{
    byte[] IManifestProcessServices.ProcessManifest(byte[] fileData)
    {
        return XorCrypto.Crypto(fileData, "YOO");
    }
}

public class TestRestoreManifest : IManifestRestoreServices
{
    byte[] IManifestRestoreServices.RestoreManifest(byte[] fileData)
    {
        return XorCrypto.Crypto(fileData, "YOO");
    }
}

public class XorCrypto
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