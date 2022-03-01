using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace YooAsset.Editor
{
	public static class AssetBundleBuilderTools
	{
		/// <summary>
		/// 检测所有损坏的预制体文件
		/// </summary>
		public static void CheckCorruptionPrefab(List<string> searchDirectorys)
		{
			if (searchDirectorys.Count == 0)
				throw new Exception("路径列表不能为空！");

			// 获取所有资源列表
			int checkCount = 0;
			int invalidCount = 0;
			string[] findAssets = EditorTools.FindAssets(EAssetSearchType.Prefab, searchDirectorys.ToArray());
			foreach (string assetPath in findAssets)
			{
				UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
				if (prefab == null)
				{
					invalidCount++;
					Debug.LogError($"发现损坏预制件：{assetPath}");
				}
				EditorTools.DisplayProgressBar("检测预制件文件是否损坏", ++checkCount, findAssets.Length);
			}
			EditorTools.ClearProgressBar();

			if (invalidCount == 0)
				Debug.Log($"没有发现损坏预制件");
		}

		/// <summary>
		/// 检测所有动画控制器的冗余状态
		/// </summary>
		public static void FindRedundantAnimationState(List<string> searchDirectorys)
		{
			if (searchDirectorys.Count == 0)
				throw new Exception("路径列表不能为空！");

			// 获取所有资源列表
			int checkCount = 0;
			int findCount = 0;
			string[] findAssets = EditorTools.FindAssets(EAssetSearchType.RuntimeAnimatorController, searchDirectorys.ToArray());
			foreach (string assetPath in findAssets)
			{
				AnimatorController animator= AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
				if (EditorTools.FindRedundantAnimationState(animator))
				{
					findCount++;
					Debug.LogWarning($"发现冗余的动画控制器：{assetPath}");
				}
				EditorTools.DisplayProgressBar("检测冗余的动画控制器", ++checkCount, findAssets.Length);
			}
			EditorTools.ClearProgressBar();

			if (findCount == 0)
				Debug.Log($"没有发现冗余的动画控制器");
			else
				AssetDatabase.SaveAssets();
		}

		/// <summary>
		/// 清理所有材质球的冗余属性
		/// </summary>
		public static void ClearMaterialUnusedProperty(List<string> searchDirectorys)
		{
			if (searchDirectorys.Count == 0)
				throw new Exception("路径列表不能为空！");

			// 获取所有资源列表
			int checkCount = 0;
			int removedCount = 0;
			string[] findAssets = EditorTools.FindAssets(EAssetSearchType.Material, searchDirectorys.ToArray());
			foreach (string assetPath in findAssets)
			{
				Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
				if (EditorTools.ClearMaterialUnusedProperty(mat))
				{
					removedCount++;
					Debug.LogWarning($"材质球已被处理：{assetPath}");
				}
				EditorTools.DisplayProgressBar("清理冗余的材质球", ++checkCount, findAssets.Length);
			}
			EditorTools.ClearProgressBar();

			if (removedCount == 0)
				Debug.Log($"没有发现冗余的材质球");
			else
				AssetDatabase.SaveAssets();
		}
	}
}