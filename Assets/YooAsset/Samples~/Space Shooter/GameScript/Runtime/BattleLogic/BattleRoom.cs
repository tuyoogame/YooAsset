using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using YooAsset;
using Random = UnityEngine.Random;

[Serializable]
public class RoomBoundary
{
	public float xMin, xMax, zMin, zMax;
}

public class BattleRoom : MonoBehaviour
{
	public static BattleRoom Instance;

	public Vector3 SpawnValues;
	public int EnemyCount = 10;
	public float SpawnWait = 0.75f;
	public float StartWait = 1f;
	public float WaveWait = 4f;

	private int _totalScore = 0;
	private bool _gameOver = false;
	private string[] _enemyLocations = new string[]
	{
		"asteroid01", "asteroid02", "asteroid03", "enemy_ship"
	};

	private AssetOperationHandle _panelHandle;

	void Awake()
	{
		Instance = this;

		var canvas = GameObject.Find("Canvas");
		_panelHandle = YooAssets.LoadAssetSync<GameObject>("UIBattle");
		var go = _panelHandle.InstantiateSync(canvas.transform);
		go.transform.localPosition = Vector3.zero;
	}
	void Start()
	{
		var handle = YooAssets.LoadAssetSync<GameObject>("player_ship");
		var go = handle.InstantiateSync();
		var bhv = go.GetComponent<EntityPlayer>();
		bhv.InitEntity(handle);

		StartCoroutine(SpawnWaves());
	}
	void OnDestroy()
	{
		Instance = null;

		if(_panelHandle != null)
		{
			_panelHandle.Release();
			_panelHandle = null;
		}
	}

	IEnumerator SpawnWaves()
	{
		yield return new WaitForSeconds(StartWait);
		while (true)
		{
			for (int i = 0; i < EnemyCount; i++)
			{
				var enemyLocation= _enemyLocations[Random.Range(0, 4)];
				Vector3 spawnPosition = new Vector3(Random.Range(-SpawnValues.x, SpawnValues.x), SpawnValues.y, SpawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;

				if(enemyLocation == "enemy_ship")
				{
					var handle = YooAssets.LoadAssetSync<GameObject>(enemyLocation);
					var go = handle.InstantiateSync(spawnPosition, spawnRotation);
					var bhv = go.GetComponent<EntityEnemy>();
					bhv.InitEntity(handle);
				}
				else
				{
					var handle = YooAssets.LoadAssetSync<GameObject>(enemyLocation);
					var go = handle.InstantiateSync(spawnPosition, spawnRotation);
					var bhv = go.GetComponent<EntityAsteroid>();
					bhv.InitEntity(handle);
				}

				yield return new WaitForSeconds(SpawnWait);
			}
			yield return new WaitForSeconds(WaveWait);

			if (_gameOver)
			{
				break;
			}
		}
	}

	public void GameOver()
	{
		_gameOver = true;
		BattleEventDispatcher.SendGameOverMsg();
	}

	public void AddScore(int score)
	{
		_totalScore += score;
		BattleEventDispatcher.SendScoreChangeMsg(_totalScore);
	}
}