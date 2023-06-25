using System;

namespace YooAsset.Editor
{
	[AttributeUsage(AttributeTargets.Class)]
	public class TaskAttribute : Attribute
	{
		/// <summary>
		/// 任务所属的构建流水线
		/// </summary>
		public ETaskPipeline Pipeline;

		/// <summary>
		/// 执行顺序
		/// </summary>
		public int TaskOrder;

		/// <summary>
		/// 任务说明
		/// </summary>
		public string TaskDesc;

		// 关联的任务类
		public Type ClassType { set; get; }

		public TaskAttribute(ETaskPipeline pipeline, int taskOrder, string taskDesc)
		{
			Pipeline = pipeline;
			TaskOrder = taskOrder;
			TaskDesc = taskDesc;
		}
	}
}