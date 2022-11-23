using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class EntityPlayer : MonoBehaviour
{
	private AssetOperationHandle _ownerHandle;

	public RoomBoundary Boundary;
	public float MoveSpeed = 10f;
	public float FireRate = 0.25f;

	private float _nextFireTime = 0f;
	private Transform _shotSpawn;
	private Rigidbody _rigidbody;
	private AudioSource _audioSource;

	public void InitEntity(AssetOperationHandle ownerHandle)
	{
		_ownerHandle = ownerHandle;
	}

	void Awake()
	{
		_rigidbody = this.gameObject.GetComponent<Rigidbody>();
		_audioSource = this.gameObject.GetComponent<AudioSource>();
		_shotSpawn = this.transform.Find("shot_spawn");
	}
	void OnDestroy()
	{
		if (_ownerHandle != null)
		{
			_ownerHandle.Release();
			_ownerHandle = null;
		}
	}
	void Update()
	{
		if (Input.GetButton("Fire1") && Time.time > _nextFireTime)
		{
			_nextFireTime = Time.time + FireRate;

			var handle = YooAssets.LoadAssetSync<GameObject>("player_bullet");
			var go = handle.InstantiateSync(_shotSpawn.position, _shotSpawn.rotation);
			var bhv = go.GetComponent<EntityBullet>();
			bhv.InitEntity(handle);

			_audioSource.Play();
		}
	}
	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");

		Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
		_rigidbody.velocity = movement * MoveSpeed;

		_rigidbody.position = new Vector3
		(
			Mathf.Clamp(GetComponent<Rigidbody>().position.x, Boundary.xMin, Boundary.xMax),
			0.0f,
			Mathf.Clamp(GetComponent<Rigidbody>().position.z, Boundary.zMin, Boundary.zMax)
		);

		float tilt = 5f;
		_rigidbody.rotation = Quaternion.Euler(0.0f, 0.0f, _rigidbody.velocity.x * -tilt);
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet") || other.CompareTag("Asteroid"))
		{
			var explosionHandle = YooAssets.LoadAssetSync<GameObject>("explosion_player");
			var go = explosionHandle.InstantiateSync(this.transform.position, this.transform.rotation);
			var bhv = go.GetComponent<EntityEffect>();
			bhv.InitEntity(explosionHandle);

			BattleRoom.Instance.GameOver();
			GameObject.Destroy(this.gameObject);
		}
	}
}