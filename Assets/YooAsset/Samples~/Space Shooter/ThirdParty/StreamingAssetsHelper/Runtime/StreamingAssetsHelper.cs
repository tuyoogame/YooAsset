//-------------------------------------
// 作者：Stark
//-------------------------------------
using UnityEngine;

public sealed class StreamingAssetsHelper
{
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
		return CurrentActivity.Call<bool>("CheckAssetExist", filePath);
	}
#else
	public static bool FileExists(string filePath)
	{
		return System.IO.File.Exists(System.IO.Path.Combine(Application.streamingAssetsPath, filePath));
	}
#endif
}