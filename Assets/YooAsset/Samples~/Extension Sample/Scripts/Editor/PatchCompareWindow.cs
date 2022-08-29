using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
	public class PatchCompareWindow : EditorWindow
	{
		static PatchCompareWindow _thisInstance;
		
		[MenuItem("YooAsset/补丁包比对工具", false, 302)]
		static void ShowWindow()
		{
			if (_thisInstance == null)
			{
				_thisInstance = EditorWindow.GetWindow(typeof(PatchCompareWindow), false, "补丁包比对工具", true) as PatchCompareWindow;
				_thisInstance.minSize = new Vector2(800, 600);
			}
			_thisInstance.Show();
		}

		private string _patchManifestPath1 = string.Empty;
		private string _patchManifestPath2 = string.Empty;
		private readonly List<PatchBundle> _changeList = new List<PatchBundle>();
		private readonly List<PatchBundle> _newList = new List<PatchBundle>();
		private Vector2 _scrollPos1;
		private Vector2 _scrollPos2;

		private void OnGUI()
		{
			GUILayout.Space(10);
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("选择补丁包1", GUILayout.MaxWidth(150)))
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
			if (GUILayout.Button("选择补丁包2", GUILayout.MaxWidth(150)))
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
				if (GUILayout.Button("比对差异", GUILayout.MaxWidth(150)))
				{
					ComparePatch(_changeList, _newList);
				}
			}

			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				int totalCount = _changeList.Count;
				EditorGUILayout.Foldout(true, $"差异列表 ( {totalCount} )");

				EditorGUI.indentLevel = 1;
				_scrollPos1 = EditorGUILayout.BeginScrollView(_scrollPos1);
				{
					foreach (var bundle in _changeList)
					{
						EditorGUILayout.LabelField($"{bundle.BundleName} | {(bundle.FileSize / 1024)}K");
					}
				}
				EditorGUILayout.EndScrollView();
				EditorGUI.indentLevel = 0;
			}

			EditorGUILayout.Space();
			using (new EditorGUI.DisabledScope(false))
			{
				int totalCount = _newList.Count;
				EditorGUILayout.Foldout(true, $"新增列表 ( {totalCount} )");

				EditorGUI.indentLevel = 1;
				_scrollPos2 = EditorGUILayout.BeginScrollView(_scrollPos2);
				{
					foreach (var bundle in _newList)
					{
						EditorGUILayout.LabelField($"{bundle.BundleName}");
					}
				}
				EditorGUILayout.EndScrollView();
				EditorGUI.indentLevel = 0;
			}
		}

		private void ComparePatch(List<PatchBundle> changeList, List<PatchBundle> newList)
		{
			changeList.Clear();
			newList.Clear();

			// 加载补丁清单1
			string jsonData1 = FileUtility.ReadFile(_patchManifestPath1);
			PatchManifest patchManifest1 = PatchManifest.Deserialize(jsonData1);

			// 加载补丁清单1
			string jsonData2 = FileUtility.ReadFile(_patchManifestPath2);
			PatchManifest patchManifest2 = PatchManifest.Deserialize(jsonData2);

			// 拷贝文件列表
			foreach (var patchBundle2 in patchManifest2.BundleList)
			{
				if (patchManifest1.TryGetPatchBundle(patchBundle2.BundleName, out PatchBundle patchBundle1))
				{
					if (patchBundle2.FileHash != patchBundle1.FileHash)
					{
						changeList.Add(patchBundle2);
					}
				}
				else
				{
					newList.Add(patchBundle2);
				}
			}

			// 按字母重新排序
			changeList.Sort((x, y) => string.Compare(x.BundleName, y.BundleName));
			newList.Sort((x, y) => string.Compare(x.BundleName, y.BundleName));

			Debug.Log("资源包差异比对完成！");
		}
	}
}
