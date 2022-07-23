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
		/// 构建管线
		/// </summary>
		public EBuildPipeline BuildPipeline = EBuildPipeline.BuiltinBuildPipeline;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode = EBuildMode.ForceRebuild;

		/// <summary>
		/// 内置资源标签（首包资源标签）
		/// </summary>
		public string BuildTags = string.Empty;

		/// <summary>
		/// 压缩方式
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.LZ4;

		/// <summary>
		/// 输出文件名称样式
		/// </summary>
		public EOutputNameStyle OutputNameStyle = EOutputNameStyle.HashName;

		/// <summary>
		/// 加密类名称
		/// </summary>
		public string EncyptionClassName = string.Empty;
	}
}