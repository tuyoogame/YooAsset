using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class PatchCombineWindow : EditorWindow
	{
		private class DependInfo
		{
			public string MainBundleName;
			public string[] DependBundleNames;
		}

		static PatchCombineWindow _thisInstance;

		[MenuItem("YooAsset/补丁包合并工具", false, 303)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(PatchCombineWindow), false, "补丁包合并工具", true) as PatchCombineWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		private string _patchManifestPath1 = string.Empty;
		private string _patchManifestPath2 = string.Empty;
		private string _patchManifestSaveFolder = string.Empty;
		private readonly Dictionary<PatchAsset, DependInfo> _dependInfos = new Dictionary<PatchAsset, DependInfo>(1000);

		private void OnGUI()
		{
			GUILayout.Space(10);
			if (GUILayout.Button("选择保存目录", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorUtility.OpenFolderPanel("Find", "Assets/", "PatchManifest");
				if (string.IsNullOrEmpty(resultPath))
					return;
				_patchManifestSaveFolder = resultPath;
			}
			EditorGUILayout.TextField("合并清单保存目录", _patchManifestSaveFolder);

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("选择补丁包（主）", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
				if (string.IsNullOrEmpty(resultPath))
					return;
				_patchManifestPath1 = resultPath;
			}
			EditorGUILayout.LabelField(_patchManifestPath1);
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("选择补丁包（副）", GUILayout.MaxWidth(150)))
			{
				string resultPath = EditorUtility.OpenFilePanel("Find", "Assets/", "bytes");
				if (string.IsNullOrEmpty(resultPath))
					return;
				_patchManifestPath2 = resultPath;
			}
			EditorGUILayout.LabelField(_patchManifestPath2);
			EditorGUILayout.EndHorizontal();

			if (string.IsNullOrEmpty(_patchManifestPath1) == false && string.IsNullOrEmpty(_patchManifestPath2) == false)
			{
				GUILayout.Space(10);
				if (GUILayout.Button("合并清单", GUILayout.MaxWidth(150)))
				{
					CombinePatch();
				}
			}
		}
		private void CombinePatch()
		{
			// 加载补丁清单1
			string jsonData1 = FileUtility.ReadFile(_patchManifestPath1);
			PatchManifest patchManifest1 = PatchManifest.Deserialize(jsonData1);

			// 加载补丁清单1
			string jsonData2 = FileUtility.ReadFile(_patchManifestPath2);
			PatchManifest patchManifest2 = PatchManifest.Deserialize(jsonData2);

			// 检测AssetPath是否冲突
			List<string> assetPathList1 = patchManifest1.AssetList.Select(t => t.AssetPath).ToList();
			List<string> assetPathList2 = patchManifest2.AssetList.Select(t => t.AssetPath).ToList();
			List<string> conflictAssetPathList = assetPathList1.Intersect(assetPathList2).ToList();
			if (conflictAssetPathList.Count > 0)
			{
				foreach (var confictAssetPath in conflictAssetPathList)
				{
					Debug.LogWarning($"资源路径冲突: {confictAssetPath}");
				}
				throw new System.Exception("资源路径冲突！请查看警告信息！");
			}

			// 检测BundleName是否冲突
			List<string> bundleNameList1 = patchManifest1.BundleList.Select(t => t.BundleName).ToList();
			List<string> bundleNameList2 = patchManifest2.BundleList.Select(t => t.BundleName).ToList();
			List<string> conflictBundleNameList = bundleNameList1.Intersect(bundleNameList2).ToList();
			if (conflictBundleNameList.Count > 0)
			{
				foreach (var confictBundleName in conflictBundleNameList)
				{
					Debug.LogWarning($"资源包名冲突: {confictBundleName}");
				}
				throw new System.Exception("资源包名冲突！请查看警告信息！");
			}

			// 记录副资源清单的依赖关系
			_dependInfos.Clear();
			foreach (var patchAsset in patchManifest2.AssetList)
			{
				string assetPath = patchAsset.AssetPath;
				var mainBundle = patchManifest2.GetMainPatchBundle(assetPath);
				var dependBundles = patchManifest2.GetAllDependencies(assetPath);
				DependInfo dependInfo = new DependInfo();
				dependInfo.MainBundleName = mainBundle.BundleName;
				dependInfo.DependBundleNames = dependBundles.Select(t => t.BundleName).ToArray();
				_dependInfos.Add(patchAsset, dependInfo);
			}

			// 副资源清单填充到主资源清单
			patchManifest1.AssetList.AddRange(patchManifest2.AssetList);
			patchManifest1.BundleList.AddRange(patchManifest2.BundleList);

			// 更新填充资源的依赖关系
			foreach (var patchAsset in _dependInfos.Keys)
			{
				patchAsset.BundleID = GetBundleID(patchManifest1, patchAsset);
				patchAsset.DependIDs = GetDependIDs(patchManifest1, patchAsset);
			}

			// 创建合并后的清单文件
			string fileSavePath = $"{_patchManifestSaveFolder}/{YooAssetSettingsData.GetPatchManifestFileName(patchManifest1.ResourceVersion)}";
			PatchManifest.Serialize(fileSavePath, patchManifest1);

			// 创建补丁清单哈希文件
			string manifestHashFilePath = $"{_patchManifestSaveFolder}/{YooAssetSettingsData.GetPatchManifestHashFileName(patchManifest1.ResourceVersion)}";
			string manifestHash = HashUtility.FileMD5(fileSavePath);
			FileUtility.CreateFile(manifestHashFilePath, manifestHash);

			Debug.Log("资源清单合并完成！");
		}

		private int GetBundleID(PatchManifest mainManifest, PatchAsset patchAsset)
		{
			if (_dependInfos.TryGetValue(patchAsset, out DependInfo dependInfo))
			{
				int index = mainManifest.BundleList.FindIndex(item => item.BundleName.Equals(dependInfo.MainBundleName));
				if (index < 0)
					throw new System.Exception("Should never get here !");
				return index;
			}
			else
			{
				throw new System.Exception("Should never get here !");
			}
		}
		private int[] GetDependIDs(PatchManifest mainManifest, PatchAsset patchAsset)
		{
			if (_dependInfos.TryGetValue(patchAsset, out DependInfo dependInfo))
			{
				List<int> results = new List<int>();
				foreach (var dependBundleName in dependInfo.DependBundleNames)
				{
					int index = mainManifest.BundleList.FindIndex(item => item.BundleName.Equals(dependBundleName));
					if (index < 0)
						throw new System.Exception("Should never get here !");
					results.Add(index);
				}
				return results.ToArray();
			}
			else
			{
				throw new System.Exception("Should never get here !");
			}
		}
	}
}