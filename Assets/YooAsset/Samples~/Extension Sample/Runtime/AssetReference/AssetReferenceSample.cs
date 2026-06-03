using System.Collections;
using UnityEngine;
using YooAsset;

/// <summary>
/// 演示如何使用 AssetReference 弱引用加载不同类型的资源
/// </summary>
public class AssetReferenceSample : MonoBehaviour
{
    [SerializeField]
    private AssetReferenceGameObject _prefabReference;

    [SerializeField]
    private AssetReferenceTexture2D _textureReference;

    private IEnumerator Start()
    {
        // 加载并实例化预制体
        if (_prefabReference.RuntimeKeyIsValid())
        {
            AssetHandle handle = _prefabReference.LoadAssetAsync();
            yield return handle;

            if (handle.Status == EOperationStatus.Succeeded)
            {
                GameObject instance = handle.InstantiateSync(new InstantiateOptions(true, transform, false));
                if (instance == null)
                    Debug.LogError($"Failed to instantiate GameObject reference '{_prefabReference.AssetGUID}'.");
            }
            else
            {
                Debug.LogError($"Failed to load GameObject reference '{_prefabReference.AssetGUID}': {handle.Error}.");
            }
        }

        // 加载纹理并赋值给渲染器材质
        if (_textureReference.RuntimeKeyIsValid())
        {
            AssetHandle handle = _textureReference.LoadAssetAsync();
            yield return handle;

            if (handle.Status == EOperationStatus.Succeeded)
            {
                var renderer = GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.mainTexture = handle.AssetObject as Texture2D;
            }
            else
            {
                Debug.LogError($"Failed to load Texture2D reference '{_textureReference.AssetGUID}': {handle.Error}.");
            }
        }
    }

    private void OnDestroy()
    {
        _prefabReference?.ReleaseAsset();
        _textureReference?.ReleaseAsset();
    }
}
