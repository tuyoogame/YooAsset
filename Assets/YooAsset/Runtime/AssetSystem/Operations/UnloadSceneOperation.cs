using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
	/// <summary>
	/// 场景卸载异步操作类
	/// </summary>
	public sealed class UnloadSceneOperation : AsyncOperationBase
	{
		private enum EFlag
		{
			Normal,
			Error,
		}
		private enum ESteps
		{
			None,
			UnLoad,
			Checking,
			Done,
		}

		private readonly EFlag _flag;
		private ESteps _steps = ESteps.None;
		private Scene _scene;
		private AsyncOperation _asyncOp;

		/// <summary>
		/// 场景卸载进度
		/// </summary>
		public float Progress
		{
			get
			{
				if (_asyncOp == null)
					return 0;
				return _asyncOp.progress;
			}
		}

		internal UnloadSceneOperation(string error)
		{
			_flag = EFlag.Error;
			Error = error;
		}
		internal UnloadSceneOperation(Scene scene)
		{
			_flag = EFlag.Normal;
			_scene = scene;
		}
		internal override void Start()
		{
			if (_flag == EFlag.Normal)
			{
				_steps = ESteps.UnLoad;
			}
			else if (_flag == EFlag.Error)
			{
				_steps = ESteps.Done;
				Status = EOperationStatus.Failed;
			}
			else
			{
				throw new System.NotImplementedException(_flag.ToString());
			}
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.UnLoad)
			{
				if (_scene.IsValid() && _scene.isLoaded)
				{
					_asyncOp = SceneManager.UnloadSceneAsync(_scene);
					_steps = ESteps.Checking;
				}
				else
				{
					Error = "Scene is invalid or is not loaded.";
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
				}
			}

			if (_steps == ESteps.Checking)
			{
				if (_asyncOp.isDone == false)
					return;

				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}
}