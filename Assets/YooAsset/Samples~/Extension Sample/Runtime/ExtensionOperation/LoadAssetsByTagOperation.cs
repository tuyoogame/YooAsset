using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

/// <summary>
/// 按资源标签加载一组资源对象
/// </summary>
/// <typeparam name="TObject">资源对象的 Unity 类型</typeparam>
public class LoadAssetsByTagOperation<TObject> : AsyncOperationBase where TObject : UnityEngine.Object
{
    private enum ESteps
    {
        None,
        LoadAssets,
        CheckResult,
        Done,
    }

    private readonly string _packageName;
    private readonly string _tag;
    private ESteps _steps = ESteps.None;
    private List<AssetHandle> _handles;
    private List<TObject> _assetObjects;

    /// <summary>
    /// 加载成功的资源对象集合
    /// </summary>
    public IReadOnlyList<TObject> AssetObjects { get { return _assetObjects; } }


    /// <summary>
    /// 创建按标签加载资源对象的操作实例
    /// </summary>
    /// <param name="packageName">资源包裹名称</param>
    /// <param name="tag">资源标签</param>
    public LoadAssetsByTagOperation(string packageName, string tag)
    {
        if (string.IsNullOrEmpty(packageName))
            throw new System.ArgumentNullException(nameof(packageName));
        if (string.IsNullOrEmpty(tag))
            throw new System.ArgumentNullException(nameof(tag));

        _packageName = packageName;
        _tag = tag;
    }
    protected override void InternalStart()
    {
        _steps = ESteps.LoadAssets;
    }
    protected override void InternalUpdate()
    {
        if (_steps == ESteps.None || _steps == ESteps.Done)
            return;

        if (_steps == ESteps.LoadAssets)
        {
            var package = YooAssets.GetPackage(_packageName);
            AssetInfo[] assetInfos = package.GetAssetInfos(_tag);
            _handles = new List<AssetHandle>(assetInfos.Length);
            foreach (var assetInfo in assetInfos)
            {
                var handle = package.LoadAssetAsync(assetInfo);
                _handles.Add(handle);
            }
            _steps = ESteps.CheckResult;
        }

        if (_steps == ESteps.CheckResult)
        {
            int index = 0;
            foreach (var handle in _handles)
            {
                if (handle.IsDone == false)
                {
                    Progress = (float)index / _handles.Count;
                    return;
                }
                index++;
            }

            _assetObjects = new List<TObject>(_handles.Count);
            foreach (var handle in _handles)
            {
                if (handle.Status == EOperationStatus.Succeeded)
                {
                    var assetObject = handle.AssetObject as TObject;
                    if (assetObject != null)
                    {
                        _assetObjects.Add(assetObject);
                    }
                    else
                    {
                        string error = $"Asset type cast failed: {handle.AssetObject.name}";
                        Debug.LogError(error);
                        _assetObjects.Clear();
                        SetFailed(error);
                        return;
                    }
                }
                else
                {
                    Debug.LogError(handle.Error);
                    _assetObjects.Clear();
                    SetFailed(handle.Error);
                    return;
                }
            }

            SetSucceed();
        }
    }
    private void SetSucceed()
    {
        SetResult();
        _steps = ESteps.Done;
    }
    private void SetFailed(string error)
    {
        SetError(error);
        _steps = ESteps.Done;
    }

    /// <summary>
    /// 释放加载过程中创建的资源句柄
    /// </summary>
    public void ReleaseHandle()
    {
        if (_handles == null)
            return;

        foreach (var handle in _handles)
        {
            handle.Release();
        }
        _handles.Clear();
    }
}