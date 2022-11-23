using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class EntityBullet : MonoBehaviour
{
	private AssetOperationHandle _ownerHandle;

	public float MoveSpeed = 20f;
	public float DelayDestroyTime = 5f;

	public void InitEntity(AssetOperationHandle ownerHandle)
	{
		_ownerHandle = ownerHandle;
	}

	void Awake()
	{
		var rigidBody = this.transform.GetComponent<Rigidbody>();
		rigidBody.velocity = this.transform.forward * MoveSpeed;
	}
	void OnDestroy()
	{
		if (_ownerHandle != null)
		{
			_ownerHandle.Release();
			_ownerHandle = null;
		}
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Boundary"))
			return;

		if (this.gameObject.CompareTag("EnemyBullet"))
		{
			if (other.CompareTag("Enemy") == false)
				GameObject.Destroy(this.gameObject);
		}

		if (this.gameObject.CompareTag("PlayerBullet"))
		{
			if (other.CompareTag("Player") == false)
				GameObject.Destroy(this.gameObject);
		}
	}
	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Boundary"))
		{
			GameObject.Destroy(this.gameObject);
		}
	}
}