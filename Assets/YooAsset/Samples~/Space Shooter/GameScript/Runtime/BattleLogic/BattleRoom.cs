using System;
using System.Collections.Generic;
using UnityEngine;
using Framework.Event;
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

    private EventSubscriptionScope _eventScope;
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

    private readonly Timer _startWaitTimer = Timer.Once(1);
    private readonly Timer _spawnWaitTimer = Timer.Once(0.75);
    private readonly Timer _waveWaitTimer = Timer.Once(4);
    private readonly List<AssetHandle> _handles = new List<AssetHandle>(1000);


    /// <summary>
    /// Initializes the room.
    /// </summary>
    public void InitRoom()
    {
        // Create room root object.
        _roomRoot = new GameObject("BattleRoom");

        // Listen for game events.
        _eventScope?.Dispose();
        _eventScope = EventManager.Instance.CreateScope();
        _eventScope.AddListener<BattlePlayerDeadEvent>(OnHandleEventMessage);
        _eventScope.AddListener<BattleEnemyDeadEvent>(OnHandleEventMessage);
        _eventScope.AddListener<BattleAsteroidExplosionEvent>(OnHandleEventMessage);
        _eventScope.AddListener<BattlePlayerFireBulletEvent>(OnHandleEventMessage);
        _eventScope.AddListener<BattleEnemyFireBulletEvent>(OnHandleEventMessage);

        _steps = ESteps.Ready;
    }

    /// <summary>
    /// Destroys the room.
    /// </summary>
    public void DestroyRoom()
    {
        _eventScope?.Dispose();
        _eventScope = null;

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
        if (message is BattlePlayerDeadEvent battlePlayerDead)
        {
            // Create explosion effect.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("explosion_player");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, battlePlayerDead.Position, battlePlayerDead.Rotation));
            };
            _handles.Add(assetHandle);

            _steps = ESteps.GameOver;
            BattleGameOverEvent.SendEventMessage();
        }
        else if (message is BattleEnemyDeadEvent battleEnemyDead)
        {
            // Create explosion effect.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("explosion_enemy");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, battleEnemyDead.Position, battleEnemyDead.Rotation));
            };
            _handles.Add(assetHandle);

            _totalScore += EnemyScore;
            BattleScoreChangedEvent.SendEventMessage(_totalScore);
        }
        else if (message is BattleAsteroidExplosionEvent battleAsteroidExplosion)
        {
            // Create explosion effect.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("explosion_asteroid");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, battleAsteroidExplosion.Position, battleAsteroidExplosion.Rotation));
            };
            _handles.Add(assetHandle);

            _totalScore += AsteroidScore;
            BattleScoreChangedEvent.SendEventMessage(_totalScore);
        }
        else if (message is BattlePlayerFireBulletEvent battlePlayerFireBullet)
        {
            // Create bullet entity.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("player_bullet");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, battlePlayerFireBullet.Position, battlePlayerFireBullet.Rotation));
            };
            _handles.Add(assetHandle);
        }
        else if (message is BattleEnemyFireBulletEvent battleEnemyFireBullet)
        {
            // Create bullet entity.
            var assetHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("enemy_bullet");
            assetHandle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(new InstantiateOptions(true, _roomRoot.transform, battleEnemyFireBullet.Position, battleEnemyFireBullet.Rotation));
            };
            _handles.Add(assetHandle);
        }
    }
}
