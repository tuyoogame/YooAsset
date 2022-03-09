
namespace YooAsset
{
	/// <summary>
	/// 资源提供者
	/// </summary>
	internal interface IAssetProvider
	{
		/// <summary>
		/// 资源路径
		/// </summary>
		string AssetPath { get; }

		/// <summary>
		/// 资源对象的名称
		/// </summary>
		string AssetName { get; }

		/// <summary>
		/// 资源对象的类型
		/// </summary>
		System.Type AssetType { get; }

		/// <summary>
		/// 获取的资源对象
		/// </summary>
		UnityEngine.Object AssetObject { get; }

		/// <summary>
		/// 获取的资源对象集合
		/// </summary>
		UnityEngine.Object[] AllAssets { get; }

		/// <summary>
		/// 扩展的实例对象
		/// </summary>
		IAssetInstance AssetInstance { get; }

		/// <summary>
		/// 当前的加载状态
		/// </summary>
		AssetProviderBase.EStatus Status { get; }

		/// <summary>
		/// 引用计数
		/// </summary>
		int RefCount { get; }

		/// <summary>
		/// 资源操作句柄
		/// </summary>
		AssetOperationHandle Handle { get; }

		/// <summary>
		/// 用户请求的回调
		/// </summary>
		System.Action<AssetOperationHandle> Callback { set; get; }

		/// <summary>
		/// 是否已经销毁
		/// </summary>
		bool IsDestroyed { get; }

		/// <summary>
		/// 是否完毕（成功或失败）
		/// </summary>
		bool IsDone { get; }

		/// <summary>
		/// 是否有效（AssetFileLoader销毁会导致Provider无效）
		/// </summary>
		bool IsValid { get; }

		/// <summary>
		/// 加载进度
		/// </summary>
		float Progress { get; }

		/// <summary>
		/// 轮询更新方法
		/// </summary>
		void Update();

		/// <summary>
		/// 销毁资源对象
		/// </summary>
		void Destory();

		/// <summary>
		/// 引用计数递加
		/// </summary>
		void Reference();

		/// <summary>
		/// 引用计数递减
		/// </summary>
		void Release();

		/// <summary>
		/// 是否可以销毁
		/// </summary>
		bool CanDestroy();

		/// <summary>
		/// 等待异步执行完毕
		/// </summary>
		void WaitForAsyncComplete();

		/// <summary>
		/// 异步操作任务
		/// </summary>
		System.Threading.Tasks.Task<object> Task { get; }
	}
}