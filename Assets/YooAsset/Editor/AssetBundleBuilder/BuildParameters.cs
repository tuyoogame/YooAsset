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
		/// 输出的根目录
		/// </summary>
		public string OutputRoot;

		/// <summary>
		/// 构建的平台
		/// </summary>
		public BuildTarget BuildTarget;

		/// <summary>
		/// 构建模式
		/// </summary>
		public EBuildMode BuildMode;

		/// <summary>
		/// 构建的版本（资源版本号）
		/// </summary>
		public int BuildVersion;

		/// <summary>
		/// 内置资源的标记列表
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
		/// 启用自动分包机制
		/// 说明：自动分包机制可以实现资源零冗余
		/// </summary>
		public bool EnableAutoCollect = true;

		/// <summary>
		/// 追加文件扩展名
		/// </summary>
		public bool AppendFileExtension = false;


		/// <summary>
		/// 加密类
		/// </summary>
		public IEncryptionServices EncryptionServices = null;

		/// <summary>
		/// 压缩选项
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.Uncompressed;

		/// <summary>
		/// 文件名附加上哈希值
		/// </summary>
		public bool AppendHash = false;

		/// <summary>
		/// 禁止写入类型树结构（可以降低包体和内存并提高加载效率）
		/// </summary>
		public bool DisableWriteTypeTree = false;

		/// <summary>
		/// 忽略类型树变化
		/// </summary>
		public bool IgnoreTypeTreeChanges = true;

		/// <summary>
		/// 禁用名称查找资源（可以降内存并提高加载效率）
		/// </summary>
		public bool DisableLoadAssetByFileName = false;


		/// <summary>
		/// 获取内置标记列表
		/// </summary>
		public List<string> GetBuildinTags()
		{
			return StringUtility.StringToStringList(BuildinTags, ';');
		}
	}
}