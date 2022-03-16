using System;
using UnityEditor;

namespace YooAsset.Editor
{
	[Serializable]
	public class BuildSummary
	{
		/// <summary>
		/// 引擎版本
		/// </summary>
		public string UnityVersion;

		/// <summary>
		/// 构建时间
		/// </summary>
		public string BuildTime;
		
		/// <summary>
		/// 构建耗时（单位：秒）
		/// </summary>
		public int BuildSeconds;

		/// <summary>
		/// 构建平台
		/// </summary>
		public BuildTarget BuildTarget;

		/// <summary>
		/// 构建版本
		/// </summary>
		public int BuildVersion;

		/// <summary>
		/// 是否开启冗余机制
		/// </summary>
		public bool ApplyRedundancy;

		/// <summary>
		/// 是否开启文件后缀名
		/// </summary>
		public bool AppendFileExtension;

		#region 着色器
		public bool IsCollectAllShaders;
		public string ShadersBundleName;
		#endregion

		#region 构建参数
		public bool IsForceRebuild;
		public string BuildinTags;
		public ECompressOption CompressOption;
		public bool IsAppendHash;
		public bool IsDisableWriteTypeTree;
		public bool IsIgnoreTypeTreeChanges;
		public bool IsDisableLoadAssetByFileName;
		#endregion
	}
}