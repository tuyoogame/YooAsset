using System.Collections;
using UnityEngine;
using YooAsset;

/// <summary>
/// 演示如何使用 GameObjectReference 弱引用加载并实例化游戏对象
/// </summary>
public class AssetReferenceSample : MonoBehaviour
{
    [SerializeField]
    private GameObjectReference _reference;

    private IEnumerator Start()
    {
        if (_reference.RuntimeKeyIsValid() == false)
        {
            yield break;
        }

        AssetHandle handle = _reference.LoadAssetAsync();
        yield return handle;

        if (handle.Status == EOperationStatus.Succeeded)
        {
            GameObject instance = handle.InstantiateSync(new InstantiateOptions(true, transform, false));
            if (instance == null)
                Debug.LogError($"Failed to instantiate GameObject reference '{_reference.AssetGUID}'.");
        }
        else
        {
            Debug.LogError($"Failed to load GameObject reference '{_reference.AssetGUID}': {handle.Error}.");
        }
    }
    private void OnDestroy()
    {
        _reference.ReleaseAsset();
    }
}
