using System;

namespace YooAsset.Editor
{
	[Serializable]
	public enum ECollectorType
	{
		/// <summary>
		/// 收集参与打包构建的资源对象，并全部写入到资源清单的资源列表里（可以通过代码加载）。
		/// </summary>
		MainCollector,

		/// <summary>
		/// 收集参与打包构建的资源对象，但不写入到资源清单的资源列表里（无法通过代码加载）。
		/// </summary>
		StaticCollector,

		/// <summary>
		/// 该收集器类型不能被设置
		/// </summary>
		None,
	}
}