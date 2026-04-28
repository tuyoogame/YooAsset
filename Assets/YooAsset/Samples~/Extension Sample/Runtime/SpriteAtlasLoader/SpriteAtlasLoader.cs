using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using YooAsset;

/// <summary>
/// 响应 Unity 图集请求并通过资源包裹加载 SpriteAtlas
/// </summary>
public class SpriteAtlasLoader : MonoBehaviour
{
    private Dictionary<string, SpriteAtlas> _loadedAtlas = new Dictionary<string, SpriteAtlas>(1000);
    private List<AssetHandle> _loadHandles = new List<AssetHandle>(1000);

    /// <summary>
    /// 注册图集请求回调
    /// </summary>
    public void Awake()
    {
        SpriteAtlasManager.atlasRequested += RequestAtlas;
    }
    /// <summary>
    /// 注销图集请求回调并释放已加载图集句柄
    /// </summary>
    public void OnDestroy()
    {
        SpriteAtlasManager.atlasRequested -= RequestAtlas;

        foreach (var handle in _loadHandles)
        {
            handle.Release();
        }
        _loadHandles.Clear();
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
                Debug.LogWarning($"Failed to load sprite atlas '{atlasName}': {loadHandle.Error}.");
                loadHandle.Release();
                callback.Invoke(null);
                return;
            }

            var atlas = loadHandle.AssetObject as SpriteAtlas;
            _loadedAtlas.Add(atlasName, atlas);
            _loadHandles.Add(loadHandle);
            callback.Invoke(atlas);
        }
    }
}
