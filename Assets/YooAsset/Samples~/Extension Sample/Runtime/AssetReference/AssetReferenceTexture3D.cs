using System;
using UnityEngine;

/// <summary>
/// Texture3D 资源弱引用
/// </summary>
[Serializable]
public class AssetReferenceTexture3D : AssetReference
{
    public override Type AssetType => typeof(Texture3D);
}
