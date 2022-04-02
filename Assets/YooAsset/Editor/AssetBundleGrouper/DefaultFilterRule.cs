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
		public bool IsCollectAsset(string assetPath)
		{
			return true;
		}
	}

	/// <summary>
	/// 只收集场景
	/// </summary>
	public class CollectScene : IFilterRule
	{
		public bool IsCollectAsset(string assetPath)
		{
			return Path.GetExtension(assetPath) == ".unity";
		}
	}
	
	/// <summary>
	/// 只收集预制体
	/// </summary>
	public class CollectPrefab : IFilterRule
	{
		public bool IsCollectAsset(string assetPath)
		{
			return Path.GetExtension(assetPath) == ".prefab";
		}
	}

	/// <summary>
	/// 只收集精灵类型的资源
	/// </summary>
	public class CollectSprite : IFilterRule
	{
		public bool IsCollectAsset(string assetPath)
		{
			if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(Sprite))
				return true;
			else
				return false;
		}
	}
}