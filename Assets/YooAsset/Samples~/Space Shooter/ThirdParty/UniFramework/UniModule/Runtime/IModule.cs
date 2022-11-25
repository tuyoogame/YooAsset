
namespace UniFramework.Module
{
	public interface IModule
	{
		/// <summary>
		/// 创建模块
		/// </summary>
		void OnCreate(System.Object createParam);

		/// <summary>
		/// 更新模块
		/// </summary>
		void OnUpdate();

		/// <summary>
		/// 销毁模块
		/// </summary>
		void OnDestroy();
	}
}