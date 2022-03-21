
namespace YooAsset
{
	public class SceneOperationHandle : OperationHandleBase
	{
		private System.Action<SceneOperationHandle> _callback;

		internal SceneOperationHandle(AssetProviderBase provider) : base(provider)
		{
		}
		internal override void InvokeCallback()
		{
			if (IsValid)
			{
				_callback?.Invoke(this);
			}
		}

		/// <summary>
		/// 完成委托
		/// </summary>
		public event System.Action<SceneOperationHandle> Completed
		{
			add
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SceneOperationHandle)} is invalid");
				if (_provider.IsDone)
					value.Invoke(this);
				else
					_callback += value;
			}
			remove
			{
				if (IsValid == false)
					throw new System.Exception($"{nameof(SceneOperationHandle)} is invalid");
				_callback -= value;
			}
		}

		/// <summary>
		/// 场景对象
		/// </summary>
		public UnityEngine.SceneManagement.Scene Scene
		{
			get
			{
				if (IsValid == false)
					return new UnityEngine.SceneManagement.Scene();
				return _provider.Scene;
			}
		}

		/// <summary>
		/// 激活场景
		/// </summary>
		public bool ActivateScene()
		{
			if (IsValid == false)
				return false;

			if (Scene.IsValid() && Scene.isLoaded)
			{
				return UnityEngine.SceneManagement.SceneManager.SetActiveScene(Scene);
			}
			else
			{
				YooLogger.Warning($"Scene is invalid or not loaded : {Scene.name}");
				return false;
			}
		}
	}
}