using System;
using UnityEngine;
using YooAsset;

/// <summary>
/// 游戏对象弱引用，序列化时只保存资源 GUID
/// </summary>
[Serializable]
public class GameObjectReference
{
    [SerializeField]
    private string _packageName = "DefaultPackage";

    [SerializeField]
    private string _assetGUID = "";

    [NonSerialized]
    private AssetHandle _handle;

    /// <summary>
    /// 资源所属的包裹名称
    /// </summary>
    public string PackageName => _packageName;

    /// <summary>
    /// 资源 GUID
    /// </summary>
    public string AssetGUID => _assetGUID;


    /// <summary>
    /// 检查运行时引用键是否有效
    /// </summary>
    public bool RuntimeKeyIsValid()
    {
        if (string.IsNullOrEmpty(_packageName) || string.IsNullOrEmpty(_assetGUID))
            return false;

        var package = YooAssets.GetPackage(_packageName);
        var assetInfo = package.GetAssetInfoByGuid(_assetGUID, typeof(GameObject));
        return assetInfo.IsValid;
    }

    /// <summary>
    /// 异步加载引用的游戏对象
    /// </summary>
    /// <returns>加载操作句柄</returns>
    public AssetHandle LoadAssetAsync()
    {
        if (_handle != null)
            throw new InvalidOperationException("GameObject reference has already been loaded. Release it first.");

        if (string.IsNullOrEmpty(_packageName))
            throw new ArgumentException("Package name is not set.", nameof(_packageName));
        if (string.IsNullOrEmpty(_assetGUID))
            throw new ArgumentException("Asset GUID is not set.", nameof(_assetGUID));

        var package = YooAssets.GetPackage(_packageName);
        var assetInfo = package.GetAssetInfoByGuid(_assetGUID, typeof(GameObject));
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
