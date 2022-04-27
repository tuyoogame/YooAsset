using System;
using UnityEngine;

namespace YooAsset.Editor
{
	public class AssetBundleBuilderSetting : ScriptableObject
	{
		/// <summary>
		/// 构建版本号
		/// </summary>
		public int BuildVersion = 0;

		/// <summary>
		/// 压缩方式
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.LZ4;

		/// <summary>
		/// 加密类名称
		/// </summary>
		public string EncyptionClassName = string.Empty;

		/// <summary>
		/// 附加后缀格式
		/// </summary>
		public bool AppendExtension = false;

		/// <summary>
		/// 强制构建
		/// </summary>
		public bool ForceRebuild = false;

		/// <summary>
		/// 演练构建
		/// </summary>
		public bool DryRunBuild = false;

		/// <summary>
		/// 内置标签
		/// </summary>
		public string BuildTags = string.Empty;
	}
}