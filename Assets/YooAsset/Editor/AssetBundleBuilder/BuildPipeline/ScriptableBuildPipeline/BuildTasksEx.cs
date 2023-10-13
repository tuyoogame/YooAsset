using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine.U2D;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

namespace UnityEditor.Build.Pipeline.Tasks
{
	/// <summary>
	/// Ref https://zhuanlan.zhihu.com/p/586918159
	/// </summary>
	public class RemoveSpriteAtlasRedundancy : IBuildTask
	{
		public int Version => 1;

		[InjectContext]
		IBundleWriteData writeDataParam;

		public ReturnCode Run()
		{
#if UNITY_2020_3_OR_NEWER
			BundleWriteData writeData = (BundleWriteData)writeDataParam;

			// 图集引用的精灵图片集合
			HashSet<GUID> spriteGuids = new HashSet<GUID>();
			foreach (var pair in writeData.FileToObjects)
			{
				foreach (ObjectIdentifier objectIdentifier in pair.Value)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(objectIdentifier.guid);
					var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
					if (assetType == typeof(SpriteAtlas))
					{
						var spritePaths = AssetDatabase.GetDependencies(assetPath, false);
						foreach (string spritePath in spritePaths)
						{
							GUID spriteGuild = AssetDatabase.GUIDFromAssetPath(spritePath);
							spriteGuids.Add(spriteGuild);
						}
					}
				}
			}

			// 移除图集引用的精力图片对象
			foreach (var pair in writeData.FileToObjects)
			{
				List<ObjectIdentifier> objectIdentifiers = pair.Value;
				for (int i = objectIdentifiers.Count - 1; i >= 0; i--)
				{
					ObjectIdentifier objectIdentifier = objectIdentifiers[i];
					if (spriteGuids.Contains(objectIdentifier.guid))
					{
						if (objectIdentifier.localIdentifierInFile == 2800000)
						{
							// 删除图集散图的冗余纹理
							objectIdentifiers.RemoveAt(i);
						}
					}
				}
			}
#endif

			return ReturnCode.Success;
		}
	}
}