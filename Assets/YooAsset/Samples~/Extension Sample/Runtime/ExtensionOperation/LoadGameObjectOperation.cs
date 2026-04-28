using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

/// <summary>
/// 提供资源包裹的游戏对象加载扩展方法
/// </summary>
public static class YooAssetsExtension
{
    /// <summary>
    /// 加载并实例化游戏对象
    /// </summary>
    /// <param name="package">发起加载的资源包裹</param>
    /// <param name="location">资源定位地址</param>
    /// <param name="position">实例化位置</param>
    /// <param name="rotation">实例化旋转</param>
    /// <param name="parent">实例化父节点</param>
    /// <param name="destroyGoOnRelease">释放句柄时是否销毁实例对象</param>
    /// <returns>游戏对象加载操作句柄</returns>
    public static LoadGameObjectOperation LoadGameObjectAsync(this ResourcePackage package, string location, Vector3 position, Quaternion rotation, Transform parent, bool destroyGoOnRelease = false)
    {
        var operation = new LoadGameObjectOperation(package.PackageName, location, position, rotation, parent, destroyGoOnRelease);
        AsyncOperationSystem.StartOperation(AsyncOperationSystem.GlobalSchedulerName, operation);
        return operation;
    }
}

/// <summary>
/// 加载并实例化游戏对象的操作
/// </summary>
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
    private readonly Vector3 _position;
    private readonly Quaternion _rotation;
    private readonly Transform _parent;
    private readonly bool _destroyGoOnRelease;
    private AssetHandle _handle;
    private ESteps _steps = ESteps.None;

    /// <summary>
    /// 加载并实例化后的游戏对象
    /// </summary>
    public GameObject GameObjectInstance { private set; get; }

    /// <summary>
    /// 创建游戏对象加载操作实例
    /// </summary>
    /// <param name="packageName">资源包裹名称</param>
    /// <param name="location">资源定位地址</param>
    /// <param name="position">实例化位置</param>
    /// <param name="rotation">实例化旋转</param>
    /// <param name="parent">实例化父节点</param>
    /// <param name="destroyGoOnRelease">释放句柄时是否销毁实例对象</param>
    public LoadGameObjectOperation(string packageName, string location, Vector3 position, Quaternion rotation, Transform parent, bool destroyGoOnRelease = false)
    {
        if (string.IsNullOrEmpty(packageName))
            throw new System.ArgumentNullException(nameof(packageName));
        if (string.IsNullOrEmpty(location))
            throw new System.ArgumentNullException(nameof(location));

        _packageName = packageName;
        _location = location;
        _position = position;
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
                SetError($"Failed to load GameObject '{_location}': {_handle.Error}.");
                _steps = ESteps.Done;
            }
            else
            {
                GameObjectInstance = _handle.InstantiateSync(new InstantiateOptions(true, _parent, _position, _rotation));
                SetResult();
                _steps = ESteps.Done;
            }
        }
    }

    /// <summary>
    /// 释放加载过程中创建的资源句柄
    /// </summary>
    public void ReleaseHandle()
    {
        if (_handle != null)
        {
            _handle.Release();

            if (_destroyGoOnRelease)
            {
                if (GameObjectInstance != null)
                    GameObject.Destroy(GameObjectInstance);
            }
        }
    }
}