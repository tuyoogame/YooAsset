using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIConsole
{
	private static Vector2 _scrollViewPos = Vector2.zero;
	private static Texture _bgTexture;
	private static bool _isInitConsole = false;
	private static bool _visible = false;
	private static readonly List<string> _logs = new List<string>(1000);
	
	private static void InitConsole()
	{
		if (_isInitConsole)
			return;
		_isInitConsole = true;

		// 监听日志
		Application.logMessageReceived += HandleUnityEngineLog;

		// 加载背景纹理
		string textureName = "console_background";
		_bgTexture = Resources.Load<Texture>(textureName);
		if (_bgTexture == null)
			UnityEngine.Debug.LogWarning($"Not found {textureName} texture in Resources folder.");
	}
	private static void HandleUnityEngineLog(string logString, string stackTrace, LogType type)
	{
		_logs.Add(logString);
		if (_logs.Count > 1000)
		{
			_logs.RemoveAt(0);
		}
	}

	public static void OnGUI()
	{
		InitConsole();

		// 绘制背景
		if (_visible && _bgTexture != null)
			GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgTexture, ScaleMode.StretchToFill, true);

		GUILayout.BeginHorizontal();
		string btnName = _visible ? "Hide" : "Show";
		if (GUI.Button(new Rect(10, 0, 60, 30), btnName))
			_visible = !_visible;
		if (GUI.Button(new Rect(100, 0, 60, 30), "Clear"))
			_logs.Clear();
		GUILayout.EndHorizontal();

		if (_visible == false)
			return;

		float scrollWidth = Screen.safeArea.width;
		float scrollHeight = Screen.safeArea.height;
		_scrollViewPos = GUILayout.BeginScrollView(_scrollViewPos, GUILayout.Width(scrollWidth), GUILayout.Height(scrollHeight));
		for (int i = 0; i < _logs.Count; i++)
		{
			GUILayout.Label($"<size={18}><color=white>{_logs[i]}</color></size>");
		}
		GUILayout.EndScrollView();
	}
}