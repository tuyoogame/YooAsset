using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using YooAsset;

public class SceneHome : MonoBehaviour
{
    public GameObject CanvasDesktop;
    private AssetHandle _windowHandle;

    void Start()
    {
        AsyncLoad();
    }

    private async void AsyncLoad()
    {
        // 加载主页面
        _windowHandle = GameManager.Instance.GamePakcage.LoadAssetAsync<GameObject>("UIHome");
        await _windowHandle;
        _windowHandle.InstantiateSync(CanvasDesktop.transform);

        // 切换场景的时候释放资源
        var package = YooAssets.GetPackage("DefaultPackage");
        var operation = package.UnloadUnusedAssetsAsync();
        await operation;
    }

    private void OnDestroy()
    {
        // 释放资源句柄
        if (_windowHandle != null)
        {
            _windowHandle.Release();
            _windowHandle = null;
        }
    }
}