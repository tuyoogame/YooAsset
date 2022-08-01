using System;
using YooAsset.Editor;

public class EncryptionNone : IEncryptionServices
{
	bool IEncryptionServices.Check(string bundleName)
	{
		return false;
	}
	byte[] IEncryptionServices.Encrypt(byte[] fileData)
	{
		throw new System.NotImplementedException();
	}
}

public class GameEncryption : IEncryptionServices
{
	/// <summary>
	/// 检测资源包是否需要加密
	/// </summary>
	bool IEncryptionServices.Check(string bundleName)
	{
		// 对配置表进行加密
		return bundleName.Contains("assets/gameres/config/");
	}

	/// <summary>
	/// 对数据进行加密，并返回加密后的数据
	/// </summary>
	byte[] IEncryptionServices.Encrypt(byte[] fileData)
	{
		int offset = 32;
		var temper = new byte[fileData.Length + offset];
		Buffer.BlockCopy(fileData, 0, temper, offset, fileData.Length);
		return temper;
	}
}