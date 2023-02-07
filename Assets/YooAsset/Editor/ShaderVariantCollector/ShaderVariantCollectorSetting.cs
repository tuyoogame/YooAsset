using UnityEngine;

namespace YooAsset.Editor
{
	public class ShaderVariantCollectorSetting : ScriptableObject
	{
		/// <summary>
		/// 文件存储路径
		/// </summary>
		public string SavePath = "Assets/MyShaderVariants.shadervariants";

		/// <summary>
		/// 收集的包裹名称
		/// </summary>
		public string CollectPackage = string.Empty;
	}
}