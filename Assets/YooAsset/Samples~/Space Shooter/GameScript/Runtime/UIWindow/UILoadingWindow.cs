using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Window;

[WindowAttribute(1000, true)]
public class UILoadingWindow : UIWindow
{
	private Text _info;
	private float _timer;
	private int _countdown;

	public override void OnCreate()
	{
		_info = this.transform.Find("info").GetComponent<Text>();
	}
	public override void OnDestroy()
	{
	}
	public override void OnRefresh()
	{
		_info.text = "Loading";
		_timer = 0;
		_countdown = 0;
	}
	public override void OnUpdate()
	{
		_timer += Time.deltaTime;
		if (_timer>= 0.1f)
		{
			_timer = 0f;
			_countdown++;
			if (_countdown > 6)
				_countdown = 0;

			string tips = "Loading";
			for(int i=0; i<_countdown; i++)
			{
				tips += ".";
			}
			_info.text = tips;
		}
	}
}