using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class EntityAsteroid : MonoBehaviour
{
	private AssetOperationHandle _ownerHandle;

	public float MoveSpeed = -5f;
	public float Tumble = 5f;

	public void InitEntity(AssetOperationHandle ownerHandle)
	{
		_ownerHandle = ownerHandle;
	}

	void Awake()
	{
		var rigidBody = this.transform.GetComponent<Rigidbody>();
		rigidBody.velocity = this.transform.forward * MoveSpeed;
		rigidBody.angularVelocity = Random.insideUnitSphere * Tumble;
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
		if (other.CompareTag("PlayerBullet"))
		{
			var handle = YooAssets.LoadAssetSync<GameObject>("explosion_asteroid");
			var go = handle.InstantiateSync(other.transform.position, Quaternion.identity);
			var bhv = go.GetComponent<EntityEffect>();
			bhv.InitEntity(handle);

			BattleRoom.Instance.AddScore(1);
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