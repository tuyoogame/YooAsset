using UnityEngine;

namespace YooAsset
{
	public class InstantiateOperation : AsyncOperationBase
	{
		private enum ESteps
		{
			None,
			Clone,
			Done,
		}

		private readonly AssetOperationHandle _handle;
		private readonly Vector3 _position;
		private readonly Quaternion _rotation;
		private readonly Transform _parent;
		private ESteps _steps = ESteps.None;

		/// <summary>
		/// 实例化的游戏对象
		/// </summary>
		public GameObject Result = null;


		internal InstantiateOperation(AssetOperationHandle handle, Vector3 position, Quaternion rotation, Transform parent)
		{
			_handle = handle;
			_position = position;
			_rotation = rotation;
			_parent = parent;
		}
		internal override void Start()
		{
			_steps = ESteps.Clone;
		}
		internal override void Update()
		{
			if (_steps == ESteps.None || _steps == ESteps.Done)
				return;

			if (_steps == ESteps.Clone)
			{
				if (_handle.IsValid == false)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"{nameof(AssetOperationHandle)} is invalid.";
				}
				if (_handle.AssetObject == null)
				{
					_steps = ESteps.Done;
					Status = EOperationStatus.Failed;
					Error = $"{nameof(AssetOperationHandle.AssetObject)} is null.";
				}

				if (_parent == null)
					Result = Object.Instantiate(_handle.AssetObject as GameObject, _position, _rotation);
				else
					Result = Object.Instantiate(_handle.AssetObject as GameObject, _position, _rotation, _parent);

				_steps = ESteps.Done;
				Status = EOperationStatus.Succeed;
			}
		}
	}
}