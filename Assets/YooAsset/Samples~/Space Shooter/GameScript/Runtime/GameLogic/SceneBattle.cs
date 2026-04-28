using System.Collections;
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
        // Load battle window.
        _windowHandle = GameManager.Instance.GamePackage.LoadAssetAsync<GameObject>("UIBattle");
        yield return _windowHandle;
        _windowHandle.InstantiateSync(new InstantiateOptions(true, CanvasDesktop.transform, false));

        // Load background music.
        _musicHandle = GameManager.Instance.GamePackage.LoadAssetAsync<AudioClip>("music_background");
        yield return _musicHandle;

        // Play background music.
        var audioSource = this.gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = _musicHandle.AssetObject as AudioClip;
        audioSource.Play();

        // Release unused assets after changing scenes.
        var package = YooAssets.GetPackage("DefaultPackage");
        var operation = package.UnloadUnusedAssetsAsync();
        yield return operation;

        _battleRoom = new BattleRoom();
        _battleRoom.InitRoom();
    }
    private void OnDestroy()
    {
        // Release asset handle.
        if (_windowHandle != null)
        {
            _windowHandle.Release();
            _windowHandle = null;
        }

        // Release asset handle.
        if (_musicHandle != null)
        {
            _musicHandle.Release();
            _musicHandle = null;
        }

        // Release battle room.
        if (_battleRoom != null)
        {
            _battleRoom.DestroyRoom();
            _battleRoom = null;
        }

        // Release unused assets after changing scenes.
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