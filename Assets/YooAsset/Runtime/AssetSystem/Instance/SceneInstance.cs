using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	/// <summary>
	/// 扩展的场景实例对象
	/// </summary>
	public class SceneInstance : IAssetInstance
	{
		private readonly AsyncOperation _asyncOp;

		public SceneInstance(AsyncOperation op)
		{
			_asyncOp = op;
		}

		/// <summary>
		/// UnityEngine场景对象
		/// </summary>
		public UnityEngine.SceneManagement.Scene Scene { internal set; get; }

		/// <summary>
		/// 激活场景
		/// </summary>
		public bool Activate()
		{
			if (Scene == null)
				return false;

			if (Scene.IsValid() && Scene.isLoaded)
			{
				return SceneManager.SetActiveScene(Scene);
			}
			else
			{
				YooLogger.Warning($"Scene is invalid or not loaded : {Scene.name}");
				return false;
			}
		}
	}

	/// <summary>
	/// 加载场景实例对象需要提供的参数类
	/// </summary>
	public class SceneInstanceParam : IAssetParam
	{
		/// <summary>
		/// 加载模式
		/// </summary>
		public LoadSceneMode LoadMode { set; get; }

		/// <summary>
		/// 物理模式
		/// </summary>
		//public LocalPhysicsMode PhysicsMode { set; get;}

		/// <summary>
		/// 加载完毕时是否主动激活
		/// </summary>
		public bool ActivateOnLoad { set; get; }
	}
}
