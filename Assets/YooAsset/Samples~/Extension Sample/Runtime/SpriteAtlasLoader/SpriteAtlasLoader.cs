using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

public class SpriteAtlasLoader : MonoBehaviour
{
    private Dictionary<string, SpriteAtlas> _loadedAtlas = new Dictionary<string, SpriteAtlas>(1000);
    private List<AssetHandle> _loadHandles = new List<AssetHandle>(1000);

    public void Awake()
    {
        SpriteAtlasManager.atlasRequested += RequestAtlas;
    }
    public void OnDestroy()
    {
        foreach (var handle in _loadHandles)
        {
            handle.Release();
        }
    }

    private void RequestAtlas(string atlasName, Action<SpriteAtlas> callback)
    {
        if (_loadedAtlas.TryGetValue(atlasName, out var value))
        {
            callback.Invoke(value);
        }
        else
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var loadHandle = package.LoadAssetSync<SpriteAtlas>(atlasName);
            if (loadHandle.Status != EOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Failed to load sprite atlas : {atlasName} ! {loadHandle.Error}");
                return;
            }

            var atlas = loadHandle.AssetObject as SpriteAtlas;
            _loadedAtlas.Add(atlasName, atlas);
            _loadHandles.Add(loadHandle);
            callback.Invoke(atlas);
        }
    }
}