using System;
using UnityEngine;
using YooAsset;

/// <summary>
/// 资源弱引用基类
/// </summary>
[Serializable]
public abstract class AssetReference
{
    [SerializeField]
    protected string _packageName = "DefaultPackage";

    [SerializeField]
    protected string _assetGUID = "";

    [NonSerialized]
    protected AssetHandle _handle;

    /// <summary>
    /// 资源所属的包裹名称
    /// </summary>
    public string PackageName => _packageName;

    /// <summary>
    /// 资源 GUID
    /// </summary>
    public string AssetGUID => _assetGUID;

    /// <summary>
    /// 当前加载句柄（未加载时为 null）
    /// </summary>
    public AssetHandle Handle => _handle;

    /// <summary>
    /// 该引用负责加载的资源类型，由子类指定
    /// </summary>
    public abstract Type AssetType { get; }


    /// <summary>
    /// 检查运行时引用键是否有效
    /// </summary>
    public bool RuntimeKeyIsValid()
    {
        if (string.IsNullOrEmpty(_packageName) || string.IsNullOrEmpty(_assetGUID))
            return false;

        var package = YooAssets.GetPackage(_packageName);
        var assetInfo = package.GetAssetInfoByGuid(_assetGUID, AssetType);
        return assetInfo.IsValid;
    }

    /// <summary>
    /// 异步加载引用的资源
    /// </summary>
    /// <returns>加载操作句柄</returns>
    public AssetHandle LoadAssetAsync()
    {
        if (_handle != null)
            throw new InvalidOperationException($"{GetType().Name} has already been loaded. Release it first.");

        if (string.IsNullOrEmpty(_packageName))
            throw new ArgumentException("Package name is not set.", nameof(_packageName));
        if (string.IsNullOrEmpty(_assetGUID))
            throw new ArgumentException("Asset GUID is not set.", nameof(_assetGUID));

        var package = YooAssets.GetPackage(_packageName);
        var assetInfo = package.GetAssetInfoByGuid(_assetGUID, AssetType);
        _handle = package.LoadAssetAsync(assetInfo);
        return _handle;
    }

    /// <summary>
    /// 释放已加载的资源句柄
    /// </summary>
    public void ReleaseAsset()
    {
        if (_handle == null)
            return;

        _handle.Release();
        _handle = null;
    }
}
