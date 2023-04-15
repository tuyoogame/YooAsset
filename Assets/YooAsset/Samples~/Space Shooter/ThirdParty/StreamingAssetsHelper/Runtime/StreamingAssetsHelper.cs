//-------------------------------------
// 作者：Stark
//-------------------------------------
using System.Collections.Generic;
using UnityEngine;

public sealed class StreamingAssetsHelper
{
	private static readonly Dictionary<string, bool> _cacheData = new Dictionary<string, bool>(1000);

#if UNITY_ANDROID && !UNITY_EDITOR
	private static AndroidJavaClass _unityPlayerClass;
	public static AndroidJavaClass UnityPlayerClass
	{
		get
		{
			if (_unityPlayerClass == null)
				_unityPlayerClass = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
			return _unityPlayerClass;
		}
	}

	private static AndroidJavaObject _currentActivity;
	public static AndroidJavaObject CurrentActivity
	{
		get
		{
			if (_currentActivity == null)
				_currentActivity = UnityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
			return _currentActivity;
		}
	}

	/// <summary>
	/// 利用安卓原生接口查询内置文件是否存在
	/// </summary>
	public static bool FileExists(string filePath)
	{
		if (_cacheData.TryGetValue(filePath, out bool result) == false)
		{
			result = CurrentActivity.Call<bool>("CheckAssetExist", filePath);
			_cacheData.Add(filePath, result);
		}
		return result;
	}
#else
	public static bool FileExists(string filePath)
	{
		if (_cacheData.TryGetValue(filePath, out bool result) == false)
		{
			result = System.IO.File.Exists(System.IO.Path.Combine(Application.streamingAssetsPath, filePath));
			_cacheData.Add(filePath, result);
		}
		return result;
	}
#endif
}