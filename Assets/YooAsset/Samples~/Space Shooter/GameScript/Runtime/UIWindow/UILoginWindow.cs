using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class UILoginWindow : MonoBehaviour
{
	void Awake()
	{
		var loginBtn = this.transform.Find("Start").GetComponent<Button>();
		loginBtn.onClick.AddListener(OnClickLoginBtn);
	}

	private void OnClickLoginBtn()
	{
		YooAssets.LoadSceneAsync("scene_game");
	}
}