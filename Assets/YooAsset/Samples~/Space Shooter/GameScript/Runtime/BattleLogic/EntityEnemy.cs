using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Random = UnityEngine.Random;

public class EntityEnemy : MonoBehaviour
{
	private const float Dodge = 5f;
	private const float Smoothing = 7.5f;

	private AssetOperationHandle _ownerHandle;

	public RoomBoundary Boundary;
	public float MoveSpeed = 20f;
	public float FireInterval = 2f;

	public Vector2 StartWait = new Vector2(0.5f, 1f);
	public Vector2 ManeuverTime = new Vector2(1, 2);
	public Vector2 ManeuverWait = new Vector2(1, 2);

	private Transform _shotSpawn;
	private Rigidbody _rigidbody;
	private AudioSource _audioSource;
	private float _lastFireTime = 0f;

	float _currentSpeed;
	float targetManeuver;

	public void InitEntity(AssetOperationHandle ownerHandle)
	{
		_ownerHandle = ownerHandle;
	}

	void Awake()
	{
		_rigidbody = this.gameObject.GetComponent<Rigidbody>();
		_audioSource = this.gameObject.GetComponent<AudioSource>();
		_shotSpawn = this.transform.Find("shot_spawn");

		_rigidbody.velocity = this.transform.forward * -5f;
		_currentSpeed = _rigidbody.velocity.z;
		StartCoroutine(Evade());
	}
	void Start()
	{
		_lastFireTime = Time.time;
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
		if (Time.time - _lastFireTime >= FireInterval)
		{
			_lastFireTime = Time.time;

			var handle = YooAssets.LoadAssetSync<GameObject>("enemy_bullet");
			var go = handle.InstantiateSync(_shotSpawn.position, _shotSpawn.rotation);
			var bhv = go.GetComponent<EntityBullet>();
			bhv.InitEntity(handle);

			_audioSource.Play();
		}
	}
	void FixedUpdate()
	{
		float newManeuver = Mathf.MoveTowards(_rigidbody.velocity.x, targetManeuver, Smoothing * Time.deltaTime);
		_rigidbody.velocity = new Vector3(newManeuver, 0.0f, _currentSpeed);
		_rigidbody.position = new Vector3
		(
			Mathf.Clamp(_rigidbody.position.x, Boundary.xMin, Boundary.xMax),
			0.0f,
			Mathf.Clamp(_rigidbody.position.z, Boundary.zMin, Boundary.zMax)
		);

		float tilt = 10f;
		_rigidbody.rotation = Quaternion.Euler(0, 0, _rigidbody.velocity.x * -tilt);
	}
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("PlayerBullet"))
		{
			var handle = YooAssets.LoadAssetSync<GameObject>("explosion_enemy");
			var go = handle.InstantiateSync(this.transform.position, this.transform.rotation);
			var bhv = go.GetComponent<EntityEffect>();
			bhv.InitEntity(handle);

			BattleRoom.Instance.AddScore(10);
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

	IEnumerator Evade()
	{
		yield return new WaitForSeconds(Random.Range(StartWait.x, StartWait.y));
		while (true)
		{
			targetManeuver = Random.Range(1, Dodge) * -Mathf.Sign(transform.position.x);
			yield return new WaitForSeconds(Random.Range(ManeuverTime.x, ManeuverTime.y));
			targetManeuver = 0;
			yield return new WaitForSeconds(Random.Range(ManeuverWait.x, ManeuverWait.y));
		}
	}
}