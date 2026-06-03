using System;
using UnityEngine;

/// <summary>
/// GameObject 资源弱引用
/// </summary>
[Serializable]
public class AssetReferenceGameObject : AssetReference
{
    public override Type AssetType => typeof(GameObject);
}
