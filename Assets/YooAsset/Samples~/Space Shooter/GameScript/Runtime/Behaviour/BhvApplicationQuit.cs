using System;
using UnityEngine;
using YooAsset;

public class BhvApplicationQuit : MonoBehaviour
{
	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}
	private void OnApplicationQuit()
	{
		YooAssets.Destroy();		//TODO wht real 是不是要调一下
	}
}