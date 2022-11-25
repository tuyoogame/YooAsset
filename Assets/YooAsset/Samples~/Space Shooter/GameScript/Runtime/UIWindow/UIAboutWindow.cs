using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Window;

[WindowAttribute(100, false)]
public class UIAboutWindow : UIWindow
{
	public override void OnCreate()
	{
		var maskBtn = this.transform.Find("mask").GetComponent<Button>();
		maskBtn.onClick.AddListener(OnClicMaskBtn);
	}
	public override void OnDestroy()
	{
	}
	public override void OnRefresh()
	{
	}
	public override void OnUpdate()
	{
	}

	private void OnClicMaskBtn()
	{
		UniWindow.CloseWindow<UIAboutWindow>();
	}
}