using UnityEngine;
using UnityEditor;
using System.IO;

namespace YooAsset.Editor
{
	/// <summary>
	/// 收集所有资源
	/// </summary>
	public class CollectAll : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return true;
		}
	}

	/// <summary>
	/// 只收集场景
	/// </summary>
	public class CollectScene : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return Path.GetExtension(data.AssetPath) == ".unity";
		}
	}
	
	/// <summary>
	/// 只收集预制体
	/// </summary>
	public class CollectPrefab : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			return Path.GetExtension(data.AssetPath) == ".prefab";
		}
	}

	/// <summary>
	/// 只收集精灵类型的资源
	/// </summary>
	public class CollectSprite : IFilterRule
	{
		public bool IsCollectAsset(FilterRuleData data)
		{
			if (AssetDatabase.GetMainAssetTypeAtPath(data.AssetPath) == typeof(Sprite))
				return true;
			else
				return false;
		}
	}
}