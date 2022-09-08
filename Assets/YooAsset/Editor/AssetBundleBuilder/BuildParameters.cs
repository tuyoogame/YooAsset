using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace YooAsset.Editor
{
	/// <summary>
	/// 构建参数
	/// </summary>
	public class BuildParameters
	{
		/// <summary>
		/// SBP构建参数
		/// </summary>
		public class SBPBuildParameters
		{
			/// <summary>
			/// 生成代码防裁剪配置
			/// </summary>
			public bool WriteLinkXML = true;

			/// <summary>
			/// 缓存服务器地址
			/// </summary>
			public string CacheServerHost;

			/// <summary>
			/// 缓存服务器端口
			/// </summary>
			public int CacheServerPort;
		}

		/// <summary>
		/// 可编程构建管线的参数
		/// </summary>
		public SBPBuildParameters SBPParameters;


		/// <summary>
		/// 输出的根目录
		/// </summary>
		public string OutputRoot;

		/// <summary>
		/// 构建的平台
		/// </summary>
		public BuildTarget BuildTarget;

		/// <summary>
		/// 构建管线
		/// </summary>
		public EBuildPipeline BuildPipeline;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode;

		/// <summary>
		/// 构建的版本（资源版本号）
		/// </summary>
		public int BuildVersion;

		/// <summary>
		/// 内置资源标签集合（首包资源标签）
		/// 注意：分号为分隔符
		/// </summary>
		public string BuildinTags;


		/// <summary>
		/// 验证构建结果
		/// </summary>
		public bool VerifyBuildingResult = false;

		/// <summary>
		/// 启用可寻址资源定位
		/// </summary>
		public bool EnableAddressable = false;
		
		/// <summary>
		/// 拷贝内置资源文件到StreamingAssets目录（首包资源文件）
		/// </summary>
		public bool CopyBuildinTagFiles = false;


		/// <summary>
		/// 加密类
		/// </summary>
		public IEncryptionServices EncryptionServices = null;

		/// <summary>
		/// 补丁文件名称的样式
		/// </summary>
		public EOutputNameStyle OutputNameStyle = EOutputNameStyle.HashName;

		/// <summary>
		/// 压缩选项
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.Uncompressed;

		/// <summary>
		/// 禁止写入类型树结构（可以降低包体和内存并提高加载效率）
		/// </summary>
		public bool DisableWriteTypeTree = false;

		/// <summary>
		/// 忽略类型树变化
		/// </summary>
		public bool IgnoreTypeTreeChanges = true;


		/// <summary>
		/// 获取内置资源标签列表（首包资源标签）
		/// </summary>
		public List<string> GetBuildinTags()
		{
			return StringUtility.StringToStringList(BuildinTags, ';');
		}
	}
}