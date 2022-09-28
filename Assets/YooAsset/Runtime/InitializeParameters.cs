using UnityEngine;

namespace YooAsset
{
	/// <summary>
	/// 初始化参数
	/// </summary>
	public abstract class InitializeParameters
	{
		/// <summary>
		/// 资源定位地址大小写不敏感
		/// </summary>
		public bool LocationToLower = false;

		/// <summary>
		/// 资源定位服务接口
		/// </summary>
		public ILocationServices LocationServices = null;

		/// <summary>
		/// 文件解密服务接口
		/// </summary>
		public IDecryptionServices DecryptionServices = null;

		/// <summary>
		/// 资源加载的最大数量
		/// </summary>
		public int AssetLoadingMaxNumber = int.MaxValue;

		/// <summary>
		/// 异步操作系统每帧允许运行的最大时间切片（单位：毫秒）
		/// </summary>
		public long OperationSystemMaxTimeSlice = long.MaxValue;
	}

	/// <summary>
	/// 编辑器下模拟运行模式的初始化参数
	/// </summary>
	public class EditorSimulateModeParameters : InitializeParameters
	{
		/// <summary>
		/// 用于模拟运行的资源清单路径
		/// 注意：如果路径为空，会自动重新构建补丁清单。
		/// </summary>
		public string SimulatePatchManifestPath;
	}

	/// <summary>
	/// 离线运行模式的初始化参数
	/// </summary>
	public class OfflinePlayModeParameters : InitializeParameters
	{
		/// <summary>
		/// 内置的资源包裹名称
		/// </summary>
		public string BuildinPackageName = string.Empty;
	}

	/// <summary>
	/// 联机运行模式的初始化参数
	/// </summary>
	public class HostPlayModeParameters : InitializeParameters
	{
		/// <summary>
		/// 默认的资源服务器下载地址
		/// </summary>
		public string DefaultHostServer;

		/// <summary>
		/// 备用的资源服务器下载地址
		/// </summary>
		public string FallbackHostServer;

#if UNITY_WEBGL
			/// <summary>
			/// WEBGL模式不支持多线程下载
			/// </summary>
			internal int BreakpointResumeFileSize = int.MaxValue;
#else
		/// <summary>
		/// 启用断点续传功能的文件大小
		/// </summary>
		public int BreakpointResumeFileSize = int.MaxValue;
#endif

		/// <summary>
		/// 下载文件校验等级
		/// </summary>
		public EVerifyLevel VerifyLevel = EVerifyLevel.High;

		/// <summary>
		/// 查询服务类
		/// </summary>
		public IQueryServices QueryServices = null;
	}
}