using System;
using System.Collections;
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
/// 战斗房间
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

    // 关卡参数
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
    /// 初始化房间
    /// </summary>
    public void IntRoom()
    {
        // 创建房间根对象
        _roomRoot = new GameObject("BattleRoom");

        // 监听游戏事件
        _eventGroup.AddListener<BattleEventDefine.PlayerDead>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleEventDefine.EnemyDead>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleEventDefine.AsteroidExplosion>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleEventDefine.PlayerFireBullet>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleEventDefine.EnemyFireBullet>(OnHandleEventMessage);

        _steps = ESteps.Ready;
    }

    /// <summary>
    /// 销毁房间
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
    /// 更新房间
    /// </summary>
    public void UpdateRoom()
    {
        if (_steps == ESteps.None || _steps == ESteps.GameOver)
            return;

        if (_steps == ESteps.Ready)
        {
            if (_startWaitTimer.Update(Time.deltaTime))
            {
                // 生成实体
                var handle = YooAssets.LoadAssetAsync<GameObject>("player_ship");
                handle.Completed += (AssetHandle handle) =>
                {
                    handle.InstantiateSync(_roomRoot.transform);
                };
                _handles.Add(handle);
                _steps = ESteps.SpawnEnemy;
            }
        }

        if (_steps == ESteps.SpawnEnemy)
        {
            var enemyLocation = _entityLocations[Random.Range(0, 4)];
            Vector3 spawnPosition = new Vector3(Random.Range(-_spawnValues.x, _spawnValues.x), _spawnValues.y, _spawnValues.z);
            Quaternion spawnRotation = Quaternion.identity;

            // 生成实体
            var handle = YooAssets.LoadAssetAsync<GameObject>(enemyLocation);
            handle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(spawnPosition, spawnRotation, _roomRoot.transform);
            };
            _handles.Add(handle);

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
    /// 接收事件
    /// </summary>
    /// <param name="message"></param>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is BattleEventDefine.PlayerDead)
        {
            var msg = message as BattleEventDefine.PlayerDead;

            // 创建爆炸效果
            var handle = YooAssets.LoadAssetAsync<GameObject>("explosion_player");
            handle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(msg.Position, msg.Rotation, _roomRoot.transform);
            };
            _handles.Add(handle);

            _steps = ESteps.GameOver;
            BattleEventDefine.GameOver.SendEventMessage();
        }
        else if (message is BattleEventDefine.EnemyDead)
        {
            var msg = message as BattleEventDefine.EnemyDead;

            // 创建爆炸效果
            var handle = YooAssets.LoadAssetAsync<GameObject>("explosion_enemy");
            handle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(msg.Position, msg.Rotation, _roomRoot.transform);
            };
            _handles.Add(handle);

            _totalScore += EnemyScore;
            BattleEventDefine.ScoreChange.SendEventMessage(_totalScore);
        }
        else if (message is BattleEventDefine.AsteroidExplosion)
        {
            var msg = message as BattleEventDefine.AsteroidExplosion;

            // 创建爆炸效果
            var handle = YooAssets.LoadAssetAsync<GameObject>("explosion_asteroid");
            handle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(msg.Position, msg.Rotation, _roomRoot.transform);
            };
            _handles.Add(handle);

            _totalScore += AsteroidScore;
            BattleEventDefine.ScoreChange.SendEventMessage(_totalScore);
        }
        else if (message is BattleEventDefine.PlayerFireBullet)
        {
            var msg = message as BattleEventDefine.PlayerFireBullet;

            // 创建子弹实体
            var handle = YooAssets.LoadAssetAsync<GameObject>("player_bullet");
            handle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(msg.Position, msg.Rotation, _roomRoot.transform);
            };
            _handles.Add(handle);
        }
        else if (message is BattleEventDefine.EnemyFireBullet)
        {
            var msg = message as BattleEventDefine.EnemyFireBullet;

            // 创建子弹实体
            var handle = YooAssets.LoadAssetAsync<GameObject>("enemy_bullet");
            handle.Completed += (AssetHandle handle) =>
            {
                handle.InstantiateSync(msg.Position, msg.Rotation, _roomRoot.transform);
            };
            _handles.Add(handle);
        }
    }
}