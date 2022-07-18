using System;
using YooAsset.Editor;

public class PackShaderVariants : IPackRule
{
	public string GetBundleName(PackRuleData data)
	{
		return "myshaders";
	}
}