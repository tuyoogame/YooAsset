using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// 记录 UI 面板依赖的图集资源
/// </summary>
public class PanelManifest : MonoBehaviour
{
	/// <summary>
	/// 面板依赖的图集列表
	/// </summary>
	public List<SpriteAtlas> ReferencesAtlas = new List<SpriteAtlas>();
}