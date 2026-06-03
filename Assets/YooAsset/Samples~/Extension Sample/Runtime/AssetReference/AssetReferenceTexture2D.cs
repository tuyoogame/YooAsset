using System;
using UnityEngine;

/// <summary>
/// Texture2D 资源弱引用
/// </summary>
[Serializable]
public class AssetReferenceTexture2D : AssetReference
{
    public override Type AssetType => typeof(Texture2D);
}
