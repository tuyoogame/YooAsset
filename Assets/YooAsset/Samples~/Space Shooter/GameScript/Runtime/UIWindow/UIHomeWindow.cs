using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniFramework.Window;

[WindowAttribute(100, false)]
public class UIHomeWindow : UIWindow
{
	public override void OnCreate()
	{
		var loginBtn = this.transform.Find("Start").GetComponent<Button>();
		loginBtn.onClick.AddListener(OnClickLoginBtn);

		var aboutBtn = this.transform.Find("About").GetComponent<Button>();
		aboutBtn.onClick.AddListener(OnClicAboutBtn);
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

	private void OnClickLoginBtn()
	{
		SceneEventDefine.ChangeToBattleScene.SendEventMessage();
	}
	private void OnClicAboutBtn()
	{
		UniWindow.OpenWindowAsync<UIAboutWindow>("UIAbout");
	}
}