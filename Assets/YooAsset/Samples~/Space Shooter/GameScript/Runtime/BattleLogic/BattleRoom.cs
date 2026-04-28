using System;
using System.Collections.Generic;
using UnityEngine;
using UniFramework.Event;
using UniFramework.Utility;
using YooAsset;
using Random = UnityEngine.Random;

[Serializable]
public class RoomBoundary
{
    public float xMin, xMax, zMin, zMax;
}

/// <summary>
/// Battle room.
/// </summary>
public class BattleRoom
{
    private enum ESteps
    {
        None,
        Ready,
        SpawnEnemy,
        WaitSpawn,
        WaitWave,
        GameOver,
    }

    private readonly EventGroup _eventGroup = new EventGroup();
    private GameObject _roomRoot;

    // Level parameters.
    private const int EnemyCount = 10;
    private const int EnemyScore = 10;
    private const int AsteroidScore = 1;
    private readonly Vector3 _spawnValues = new Vector3(6, 0, 20);
    private readonly string[] _entityLocations = new string[]
    {
        "asteroid01", "asteroid02", "asteroid03", "enemy_ship"
    };

    private ESteps _steps = ESteps.None;
    private int _totalScore = 0;
    private int _waveSpawnCount = 0;

    private readonly UniTimer _startWaitTimer = UniTimer.CreateOnceTimer(1f);
    private readonly UniTimer _spawnWaitTimer = UniTimer.CreateOnceTimer(0.75f);
    private readonly UniTimer _waveWaitTimer = UniTimer.CreateOnceTimer(4f);
    private readonly List<AssetHandle> _handles = new List<AssetHandle>(1000);


    /// <summary>
    /// Initializes the room.
    /// </summary>
    public void InitRoom()
    {
        // Create room root object.
        _roomRoot = new GameObject("BattleRoom");

        // Listen for game events.
        _eventGroup.AddListener<BattlePlayerDeadEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleEnemyDeadEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleAsteroidExplosionEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<BattlePlayerFireBulletEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleEnemyFireBulletEvent>(OnHandleEventMessage);

        _steps = ESteps.Ready;
    }

    /// <summary>
    /// Destroys the room.
    /// </summary>
    public void DestroyRoom()
    {
        if (_eventGroup != null)
            _eventGroup.RemoveAllListener();

        if (_roomRoot != null)
            GameObject.Destroy(_roomRoot);

        foreach(var handle in _handles)
        {
            handle.Release();
        }
        _handles.Clear();
    }

    /// <summary>
    /// Updates the room.
    /// </summary>
    public void UpdateRoom()
    {
        if (_steps == ESteps.None || _steps == ESteps.GameOver)
            return;

        if (_steps == ESteps.Ready)
        {
            if (_startWaitTimer.Update(Time.deltaTime))
            {
                // Spawn entity.
                var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("player_ship");
                assetHandle.Completed += (AssetHandle handle) =>
                {
                    handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, false));
                };
                _handles.Add(assetHandle);
                _steps = ESteps.SpawnEnemy;
            }
        }

        if (_steps == ESteps.SpawnEnemy)
        {
            var enemyLocation = _entityLocations[Random.Range(0, 4)];
            Vector3 spawnPosition = new Vector3(Random.Range(-_spawnValues.x, _spawnValues.x), _spawnValues.y, _spawnValues.z);
            Quaternion spawnRotation = Quaternion.identity;

            // Spawn entity.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>(enemyLocation);
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, spawnPosition, spawnRotation));
            };
            _handles.Add(assetHandle);

            _waveSpawnCount++;
            if (_waveSpawnCount >= EnemyCount)
            {
                _steps = ESteps.WaitWave;
            }
            else
            {
                _steps = ESteps.WaitSpawn;
            }
        }

        if (_steps == ESteps.WaitSpawn)
        {
            if (_spawnWaitTimer.Update(Time.deltaTime))
            {
                _spawnWaitTimer.Reset();
                _steps = ESteps.SpawnEnemy;
            }
        }

        if (_steps == ESteps.WaitWave)
        {
            if (_waveWaitTimer.Update(Time.deltaTime))
            {
                _waveWaitTimer.Reset();
                _waveSpawnCount = 0;
                _steps = ESteps.SpawnEnemy;
            }
        }
    }

    /// <summary>
    /// Handles event messages.
    /// </summary>
    /// <param name="message"></param>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is BattlePlayerDeadEvent)
        {
            var msg = message as BattlePlayerDeadEvent;

            // Create explosion effect.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("explosion_player");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, msg.Position, msg.Rotation));
            };
            _handles.Add(assetHandle);

            _steps = ESteps.GameOver;
            BattleGameOverEvent.SendEventMessage();
        }
        else if (message is BattleEnemyDeadEvent)
        {
            var msg = message as BattleEnemyDeadEvent;

            // Create explosion effect.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("explosion_enemy");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, msg.Position, msg.Rotation));
            };
            _handles.Add(assetHandle);

            _totalScore += EnemyScore;
            BattleScoreChangedEvent.SendEventMessage(_totalScore);
        }
        else if (message is BattleAsteroidExplosionEvent)
        {
            var msg = message as BattleAsteroidExplosionEvent;

            // Create explosion effect.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("explosion_asteroid");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, msg.Position, msg.Rotation));
            };
            _handles.Add(assetHandle);

            _totalScore += AsteroidScore;
            BattleScoreChangedEvent.SendEventMessage(_totalScore);
        }
        else if (message is BattlePlayerFireBulletEvent)
        {
            var msg = message as BattlePlayerFireBulletEvent;

            // Create bullet entity.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("player_bullet");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, msg.Position, msg.Rotation));
            };
            _handles.Add(assetHandle);
        }
        else if (message is BattleEnemyFireBulletEvent)
        {
            var msg = message as BattleEnemyFireBulletEvent;

            // Create bullet entity.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("enemy_bullet");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, msg.Position, msg.Rotation));
            };
            _handles.Add(assetHandle);
        }
    }
}