using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

public static class YooAssetsExtension
{
    public static LoadGameObjectOperation LoadGameObjectAsync(this ResourcePackage package, string location, Vector3 position, Quaternion rotation, Transform parent, bool destroyGoOnRelease = false)
    {
        var operation = new LoadGameObjectOperation(package.PackageName, location, position, rotation, parent, destroyGoOnRelease);
        AsyncOperationSystem.StartOperation(AsyncOperationSystem.GlobalSchedulerName, operation);
        return operation;
    }
}

public class LoadGameObjectOperation : AsyncOperationBase
{
    private enum ESteps
    {
        None,
        LoadAsset,
        Done,
    }

    private readonly string _packageName;
    private readonly string _location;
    private readonly Vector3 _positon;
    private readonly Quaternion _rotation;
    private readonly Transform _parent;
    private readonly bool _destroyGoOnRelease;
    private AssetHandle _handle;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 加载的游戏对象
    /// </summary>
    public GameObject Go { private set; get; }


    public LoadGameObjectOperation(string packageName, string location, Vector3 position, Quaternion rotation, Transform parent, bool destroyGoOnRelease = false)
    {
        _packageName = packageName;
        _location = location;
        _positon = position;
        _rotation = rotation;
        _parent = parent;
        _destroyGoOnRelease = destroyGoOnRelease;
    }
    protected override void InternalStart()
    {
        _steps = ESteps.LoadAsset;
    }
    protected override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.LoadAsset)
        {
            if (_handle == null)
            {
                var package = YooAssets.GetPackage(_packageName);
                _handle = package.LoadAssetAsync<GameObject>(_location);
            }

            Progress = _handle.Progress;
            if (_handle.IsDone == false)
                return;

            if (_handle.Status != EOperationStatus.Succeeded)
            {
                SetError(_handle.Error);
                _steps = ESteps.Done;
            }
            else
            {
                Go = _handle.InstantiateSync(_positon, _rotation, _parent);
                SetResult();
                _steps = ESteps.Done;
            }
        }
    }

    /// <summary>
    /// 释放资源句柄
    /// </summary>
    public void ReleaseHandle()
    {
        if (_handle != null)
        {
            _handle.Release();

            if (_destroyGoOnRelease)
            {
                if (Go != null)
                    GameObject.Destroy(Go);
            }
        }
    }
}