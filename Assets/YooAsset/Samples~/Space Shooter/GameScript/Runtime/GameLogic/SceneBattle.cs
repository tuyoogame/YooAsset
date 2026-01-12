using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

internal class SceneBattle : MonoBehaviour
{
    public GameObject CanvasDesktop;

    private AssetHandle _windowHandle;
    private AssetHandle _musicHandle;
    private BattleRoom _battleRoom;

    private IEnumerator Start()
    {
        // 加载战斗页面
        _windowHandle = GameManager.Instance.GamePakcage.LoadAssetAsync<GameObject>("UIBattle");
        yield return _windowHandle;
        _windowHandle.InstantiateSync(CanvasDesktop.transform);

        // 加载背景音乐
        _musicHandle = GameManager.Instance.GamePakcage.LoadAssetAsync<AudioClip>("music_background");
        yield return _musicHandle;

        // 播放背景音乐
        var audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = _musicHandle.AssetObject as AudioClip;
        audioSource.Play();

        // 切换场景的时候释放资源
        var package = YooAssets.GetPackage("DefaultPackage");
        var operation = package.UnloadUnusedAssetsAsync();
        yield return operation;

        _battleRoom = new BattleRoom();
        _battleRoom.IntRoom();
    }
    private void OnDestroy()
    {
        // 释放资源句柄
        if (_windowHandle != null)
        {
            _windowHandle.Release();
            _windowHandle = null;
        }

        // 释放资源句柄
        if (_musicHandle != null)
        {
            _musicHandle.Release();
            _musicHandle = null;
        }

        // 释放资源句柄
        if (_battleRoom != null)
        {
            _battleRoom.DestroyRoom();
            _battleRoom = null;
        }

        // 切换场景的时候释放资源
        if (YooAssets.IsInitialized)
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.UnloadUnusedAssetsAsync();
            operation.WaitForCompletion();
        }
    }
    private void Update()
    {
        if (_battleRoom != null)
            _battleRoom.UpdateRoom();
    }
}