using System.IO;

namespace YooAsset.Editor
{
	[DisplayName("以文件名称为定位地址")]
	public class AddressByFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			return Path.GetFileNameWithoutExtension(data.AssetPath);
		}
	}

	[DisplayName("以分组名称+文件名称为定位地址")]
	public class AddressByGroupAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			return $"{data.GroupName}_{fileName}";
		}
	}

	[DisplayName("以收集器名称+文件名称为定位地址")]
	public class AddressByCollectorAndFileName : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
			string collectorName = Path.GetFileNameWithoutExtension(data.CollectPath);
			return $"{collectorName}_{fileName}";
		}
	}


	[DisplayName("以Address+文件路径为定位地址")]
	public class AddressByAddressAndFilePath : IAddressRule
	{
		string IAddressRule.GetAssetAddress(AddressRuleData data)
		{
			if (Path.HasExtension(data.CollectPath))
			{
				return data.Address;
			}
			else
			{
				string path = data.AssetPath.Replace(data.CollectPath, "");
				if (data.IsMultiPlatform)
				{
					string platform = "Windows";
#if UNITY_ANDROID
					platform = "Android";
#elif UNITY_IOS
					platform = "iOS";
#elif UNITY_STANDALONE_OSX
					platform = "OSX";
#endif
					path = path.Replace($"{platform}/", "");
				}
				string fileName = Path.GetFileName(data.AssetPath);
				return $"{data.Address}{path}";
			}

		}
	}
}