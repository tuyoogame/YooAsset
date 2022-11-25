using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Pooling;

public class EntityBullet : MonoBehaviour
{
	public float MoveSpeed = 20f;
	public float DelayDestroyTime = 5f;

	private SpawnHandle _handle;
	private Rigidbody _rigidbody;

	public void InitEntity(SpawnHandle handle)
	{
		_handle = handle;
		_rigidbody.velocity = this.transform.forward * MoveSpeed;
	}

	void Awake()
	{
		_rigidbody = this.transform.GetComponent<Rigidbody>();	
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Boundary"))
			return;

		if (this.gameObject.CompareTag("EnemyBullet"))
		{
			if (other.CompareTag("Enemy") == false)
			{
				_handle.Restore();
				_handle = null;
			}
		}

		if (this.gameObject.CompareTag("PlayerBullet"))
		{
			if (other.CompareTag("Player") == false)
			{
				_handle.Restore();
				_handle = null;
			}
		}
	}
	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Boundary"))
		{
			_handle.Restore();
			_handle = null;
		}
	}
}