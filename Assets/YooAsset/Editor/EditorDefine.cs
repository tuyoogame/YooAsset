
using System;

namespace YooAsset.Editor
{
	public class EditorDefine
	{
		/// <summary>
		/// 资源包构建工具的配置文件存储路径
		/// </summary>
		public const string AssetBundleBuilderSettingFilePath = "Assets/YooAssetSetting/AssetBundleBuilderSetting.asset";

		/// <summary>
		/// 资源包分组工具的配置文件存储路径
		/// </summary>
		public const string AssetBundleGrouperSettingFilePath = "Assets/YooAssetSetting/AssetBundleGrouperSetting.asset";

		/// <summary>
		/// 着色器变种收集工具的配置文件存储路径
		/// </summary>
		public const string ShaderVariantCollectorSettingFilePath = "Assets/YooAssetSetting/ShaderVariantCollectorSetting.asset";

		/// <summary>
		/// 停靠窗口类型集合
		/// </summary>
		public static readonly Type[] DockedWindowTypes = { typeof(AssetBundleBuilderWindow), typeof(AssetBundleGrouperWindow), typeof(AssetBundleDebuggerWindow), typeof(AssetBundleReporterWindow)};
	}

	/// <summary>
	/// 资源搜索类型
	/// </summary>
	public enum EAssetSearchType
	{
		All,
		RuntimeAnimatorController,
		AnimationClip,
		AudioClip,
		AudioMixer,
		Font,
		Material,
		Mesh,
		Model,
		PhysicMaterial,
		Prefab,
		Scene,
		Script,
		Shader,
		Sprite,
		Texture,
		VideoClip,
	}

	/// <summary>
	/// 资源文件格式
	/// </summary>
	public enum EAssetFileExtension
	{
		prefab, //预制体
		unity, //场景
		fbx, //模型
		anim, //动画
		controller, //控制器
		png, //图片
		jpg, //图片
		mat, //材质球
		shader, //着色器
		ttf, //字体
		cs, //脚本
	}
}