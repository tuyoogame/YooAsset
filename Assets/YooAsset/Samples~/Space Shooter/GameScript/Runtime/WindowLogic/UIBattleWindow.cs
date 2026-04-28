using UnityEngine;
using UnityEngine.UI;
using UniFramework.Event;

public class UIBattleWindow : MonoBehaviour
{
    private readonly EventGroup _eventGroup = new EventGroup();
    private GameObject _overView;
    private Text _scoreLabel;

    private void Awake()
    {
        _overView = this.transform.Find("OverView").gameObject;
        _scoreLabel = this.transform.Find("ScoreView/Score").GetComponent<Text>();
        _scoreLabel.text = "Score: 0";

        var restartBtn = this.transform.Find("OverView/ReplayButton").GetComponent<Button>();
        restartBtn.onClick.AddListener(OnClickReplayBtn);

        var homeBtn = this.transform.Find("OverView/HomeButton").GetComponent<Button>();
        homeBtn.onClick.AddListener(OnClickHomeBtn);

        _eventGroup.AddListener<BattleScoreChangedEvent>(OnHandleEventMessage);
        _eventGroup.AddListener<BattleGameOverEvent>(OnHandleEventMessage);
    }
    private void OnDestroy()
    {
        _eventGroup.RemoveAllListener();
    }

    private void OnClickReplayBtn()
    {
        SceneChangeToBattleEvent.SendEventMessage();
    }
    private void OnClickHomeBtn()
    {
        SceneChangeToHomeEvent.SendEventMessage();
    }
    private void OnHandleEventMessage(IEventMessage message)
    {
        if(message is BattleScoreChangedEvent)
        {
            var msg = message as BattleScoreChangedEvent;
            _scoreLabel.text = $"Score: {msg.CurrentScores}";
        }
        else if(message is BattleGameOverEvent)
        {
            _overView.SetActive(true);
        }
    }
}