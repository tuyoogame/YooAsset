using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

public class UIBattleWindow : MonoBehaviour
{
	private Text _scoreLabel;
	private GameObject _overView;

	void Awake()
	{
		_scoreLabel = this.transform.Find("ScoreView/Score").GetComponent<Text>();
		_scoreLabel.text = "Score : 0";
	
		var restartBtn = this.transform.Find("OverView/Restart").GetComponent<Button>();
		restartBtn.onClick.AddListener(OnClickLoginBtn);

		var homeBtn = this.transform.Find("OverView/Home").GetComponent<Button>();
		homeBtn.onClick.AddListener(OnClickHomeBtn);

		_overView = this.transform.Find("OverView").gameObject;
		_overView.gameObject.SetActive(false);

		EventManager.AddListener<BattleEventDefine.ScoreChange>(OnHandleEventMessage);
		EventManager.AddListener<BattleEventDefine.GameOver>(OnHandleEventMessage);
	}
	void OnDestroy()
	{
		EventManager.RemoveListener<BattleEventDefine.ScoreChange > (OnHandleEventMessage);
		EventManager.RemoveListener<BattleEventDefine.GameOver>(OnHandleEventMessage);
	}

	private void OnClickLoginBtn()
	{
		YooAssets.LoadSceneAsync("scene_game");
	}
	private void OnClickHomeBtn()
	{
		YooAssets.LoadSceneAsync("scene_home");
	}
	private void OnHandleEventMessage(IEventMessage message)
	{
		if(message is BattleEventDefine.ScoreChange)
		{
			var msg = message as BattleEventDefine.ScoreChange;
			_scoreLabel.text = $"Score : {msg.CurrentScores}";
		}
		else if(message is BattleEventDefine.GameOver)
		{
			_overView.SetActive(true);
		}
	}
}