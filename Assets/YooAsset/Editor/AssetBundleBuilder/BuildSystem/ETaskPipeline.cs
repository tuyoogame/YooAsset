using System;

namespace YooAsset.Editor
{
	public enum ETaskPipeline 
	{
		/// <summary>
		/// 所有的构建管线
		/// </summary>
		AllPipeline,

		/// <summary>
		/// 内置构建管线
		/// </summary>
		BuiltinBuildPipeline,

		/// <summary>
		/// 可编程构建管线
		/// </summary>
		ScriptableBuildPipeline,
	}
}