using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using YooAsset;

public class Game2Scene : MonoBehaviour
{
	public GameObject CanvasRoot;
	private readonly List<AssetOperationHandle> _cachedAssetOperationHandles = new List<AssetOperationHandle>(1000);
	private SceneOperationHandle _subSceneHandle = null;

	void Start()
	{
		YooAssets.UnloadUnusedAssets();

		// 初始化窗口
		InitWindow();

		// 异步加载背景音乐
		StartCoroutine(AsyncLoadMusic());
	}
	void OnDestroy()
	{
		foreach (var handle in _cachedAssetOperationHandles)
		{
			handle.Release();
		}
		_cachedAssetOperationHandles.Clear();
	}
	void OnGUI()
	{
		GUIConsole.OnGUI();
	}

	void InitWindow()
	{
		// 同步加载背景图片
#if UNITY_WEBGL
		{
			var rawImage = CanvasRoot.transform.Find("background").GetComponent<RawImage>();
			AssetOperationHandle handle = YooAssets.LoadAssetAsync<Texture>("Texture/bg");
			_cachedAssetOperationHandles.Add(handle);
			handle.Completed += (AssetOperationHandle obj) =>
			{
				rawImage.texture = handle.AssetObject as Texture;
			};
		}
#else
		{
			var rawImage = CanvasRoot.transform.Find("background").GetComponent<RawImage>();
			AssetOperationHandle handle = YooAssets.LoadAssetSync<Texture>("Texture/bg");
			_cachedAssetOperationHandles.Add(handle);
			rawImage.texture = handle.AssetObject as Texture;
		}
#endif

		// 异步加载主场景
		{
			var btn = CanvasRoot.transform.Find("load_scene").GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				YooAssets.LoadSceneAsync("Scene/Game1");
			});
		}

		// 异步加载子场景
		{
			var btn = CanvasRoot.transform.Find("subSceneLoadBtn").GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				_subSceneHandle = YooAssets.LoadSceneAsync("Scene/SubScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
			});
		}

		// 异步卸载子场景
		{
			var btn = CanvasRoot.transform.Find("subSceneUnloadBtn").GetComponent<Button>();
			btn.onClick.AddListener(() =>
			{
				if(_subSceneHandle != null)
				{
					_subSceneHandle.UnloadAsync();
					_subSceneHandle = null;
				}				
			});
		}
	}

	/// <summary>
	/// 异步加载背景音乐
	/// </summary>
	IEnumerator AsyncLoadMusic()
	{
		// 加载背景音乐
		{
			var audioSource = CanvasRoot.transform.Find("music").GetComponent<AudioSource>();
			AssetOperationHandle handle = YooAssets.LoadAssetAsync<AudioClip>("Music/town");
			_cachedAssetOperationHandles.Add(handle);
			yield return handle;
			audioSource.clip = handle.AssetObject as AudioClip;
			audioSource.Play();
		}
	}
}