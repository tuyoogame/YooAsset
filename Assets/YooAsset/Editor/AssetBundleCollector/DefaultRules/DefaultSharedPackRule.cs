using System;
using System.IO;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 零冗余共享资源打包规则
	/// </summary>
	public class DefaultSharedPackRule : ISharedPackRule
	{
		public PackRuleResult GetPackRuleResult(string assetPath)
		{
			string bundleName = Path.GetDirectoryName(assetPath);
			PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
			return result;
		}
	}
}