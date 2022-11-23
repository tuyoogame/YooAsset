using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class EntityEffect : MonoBehaviour
{
	private AssetOperationHandle _ownerHandle;

	public float DelayDestroyTime = 1f;

	public void InitEntity(AssetOperationHandle ownerHandle)
	{
		_ownerHandle = ownerHandle;
	}
	
	void Awake()
	{
		Invoke(nameof(DelayDestroy), DelayDestroyTime);
	}
	void OnDestroy()
	{
		if (_ownerHandle != null)
		{
			_ownerHandle.Release();
			_ownerHandle = null;
		}
	}

	private void DelayDestroy()
	{
		GameObject.Destroy(this.gameObject);
	}
}