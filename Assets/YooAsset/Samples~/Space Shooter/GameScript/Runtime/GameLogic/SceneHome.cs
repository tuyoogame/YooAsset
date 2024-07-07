using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public class SceneHome : MonoBehaviour
{
    public GameObject CanvasDesktop;
    private AssetHandle _windowHandle;

#if UNITY_WEBGL
    private IEnumerator Start()
    {
        // 同步加载登录页面
        _windowHandle = YooAssets.LoadAssetAsync<GameObject>("UIHome");
        yield return _windowHandle;
        _windowHandle.InstantiateSync(CanvasDesktop.transform);
    }
#else
    private void Start()
    {
        // 异步加载登录页面
        _windowHandle = YooAssets.LoadAssetSync<GameObject>("UIHome");
        _windowHandle.InstantiateSync(CanvasDesktop.transform);
    }
#endif


    private void OnDestroy()
    {
        if (_windowHandle != null)
        {
            _windowHandle.Release();
            _windowHandle = null;
        }

        // 切换场景的时候释放资源
        if (YooAssets.Initialized)
        {
            var package = YooAssets.GetPackage("DefaultPackage");
            var operation = package.UnloadUnusedAssetsAsync();
            operation.WaitForAsyncComplete();
        }
    }
}