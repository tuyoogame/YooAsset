using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	public class BuildBundleInfo
	{
		public class BuildPatchInfo
		{
			/// <summary>
			/// 构建内容的哈希值
			/// </summary>
			public string ContentHash { set; get; }

			/// <summary>
			/// 文件哈希值
			/// </summary>
			public string PatchFileHash { set; get; }

			/// <summary>
			/// 文件哈希值
			/// </summary>
			public string PatchFileCRC { set; get; }

			/// <summary>
			/// 文件哈希值
			/// </summary>
			public long PatchFileSize { set; get; }


			/// <summary>
			/// 构建输出的文件路径
			/// </summary>
			public string BuildOutputFilePath { set; get; }

			/// <summary>
			/// 补丁包输出文件路径
			/// </summary>
			public string PatchOutputFilePath { set; get; }
		}

		/// <summary>
		/// 资源包名称
		/// </summary>
		public string BundleName { private set; get; }

		/// <summary>
		/// 参与构建的资源列表
		/// 注意：不包含零依赖资源
		/// </summary>
		public readonly List<BuildAssetInfo> BuildinAssets = new List<BuildAssetInfo>();

		/// <summary>
		/// 补丁文件信息
		/// </summary>
		public readonly BuildPatchInfo PatchInfo = new BuildPatchInfo();

		/// <summary>
		/// Bundle文件的加载方法
		/// </summary>
		public EBundleLoadMethod LoadMethod { set; get; }

		/// <summary>
		/// 加密生成文件的路径
		/// 注意：如果未加密该路径为空
		/// </summary>
		public string EncryptedFilePath { set; get; }

		/// <summary>
		/// 是否为原生文件
		/// </summary>
		public bool IsRawFile
		{
			get
			{
				foreach (var asset in BuildinAssets)
				{
					if (asset.IsRawAsset)
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// 是否为加密文件
		/// </summary>
		public bool IsEncryptedFile
		{
			get
			{
				if (string.IsNullOrEmpty(EncryptedFilePath))
					return false;
				else
					return true;
			}
		}


		public BuildBundleInfo(string bundleName)
		{
			BundleName = bundleName;
		}

		/// <summary>
		/// 添加一个打包资源
		/// </summary>
		public void PackAsset(BuildAssetInfo assetInfo)
		{
			if (IsContainsAsset(assetInfo.AssetPath))
				throw new System.Exception($"Asset is existed : {assetInfo.AssetPath}");

			BuildinAssets.Add(assetInfo);
		}

		/// <summary>
		/// 是否包含指定资源
		/// </summary>
		public bool IsContainsAsset(string assetPath)
		{
			foreach (var assetInfo in BuildinAssets)
			{
				if (assetInfo.AssetPath == assetPath)
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 获取资源包的分类标签列表
		/// </summary>
		public string[] GetBundleTags()
		{
			List<string> result = new List<string>(BuildinAssets.Count);
			foreach (var assetInfo in BuildinAssets)
			{
				foreach (var assetTag in assetInfo.BundleTags)
				{
					if (result.Contains(assetTag) == false)
						result.Add(assetTag);
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// 获取构建的资源路径列表
		/// </summary>
		public string[] GetBuildinAssetPaths()
		{
			return BuildinAssets.Select(t => t.AssetPath).ToArray();
		}

		/// <summary>
		/// 获取所有写入补丁清单的资源
		/// </summary>
		public BuildAssetInfo[] GetAllPatchAssetInfos()
		{
			return BuildinAssets.Where(t => t.CollectorType == ECollectorType.MainAssetCollector).ToArray();
		}

		/// <summary>
		/// 创建AssetBundleBuild类
		/// </summary>
		public UnityEditor.AssetBundleBuild CreatePipelineBuild()
		{
			// 注意：我们不在支持AssetBundle的变种机制
			AssetBundleBuild build = new AssetBundleBuild();
			build.assetBundleName = BundleName;
			build.assetBundleVariant = string.Empty;
			build.assetNames = GetBuildinAssetPaths();
			return build;
		}

		/// <summary>
		/// 创建PatchBundle类
		/// </summary>
		internal PatchBundle CreatePatchBundle()
		{
			string fileHash = PatchInfo.PatchFileHash;
			string fileCRC = PatchInfo.PatchFileCRC;
			long fileSize = PatchInfo.PatchFileSize;
			bool isRawFile = IsRawFile;
			byte loadMethod = (byte)LoadMethod;
			string[] tags = GetBundleTags();
			PatchBundle patchBundle = new PatchBundle(BundleName, fileHash, fileCRC, fileSize, isRawFile, loadMethod, tags);
			return patchBundle;
		}
	}
}