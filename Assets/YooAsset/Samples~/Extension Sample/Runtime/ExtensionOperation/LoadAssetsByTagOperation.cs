using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;

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

    /// <summary>
    /// 资源对象集合
    /// </summary>
    public List<TObject> AssetObjects { private set; get; }


    public LoadAssetsByTagOperation(string packageName, string tag)
    {
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

            AssetObjects = new List<TObject>(_handles.Count);
            foreach (var handle in _handles)
            {
                if (handle.Status == EOperationStatus.Succeeded)
                {
                    var assetObject = handle.AssetObject as TObject;
                    if (assetObject != null)
                    {
                        AssetObjects.Add(assetObject);
                    }
                    else
                    {
                        string error = $"资源类型转换失败：{handle.AssetObject.name}";
                        Debug.LogError($"{error}");
                        AssetObjects.Clear();
                        SetFailed(error);
                        return;
                    }
                }
                else
                {
                    Debug.LogError($"{handle.Error}");
                    AssetObjects.Clear();
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
    /// 释放资源句柄
    /// </summary>
    public void ReleaseHandle()
    {
        foreach (var handle in _handles)
        {
            handle.Release();
        }
        _handles.Clear();
    }
}