using UnityEngine;
using UnityEngine.UI;
using Framework.Event;

public class UIBattleWindow : MonoBehaviour
{
    private EventSubscriptionScope _eventScope;
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

        _eventScope = EventManager.Instance.CreateScope();
        _eventScope.AddListener<BattleScoreChangedEvent>(OnHandleEventMessage);
        _eventScope.AddListener<BattleGameOverEvent>(OnHandleEventMessage);
    }
    private void OnDestroy()
    {
        _eventScope?.Dispose();
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
        if (message is BattleScoreChangedEvent scoreChanged)
        {
            _scoreLabel.text = $"Score: {scoreChanged.CurrentScores}";
        }
        else if (message is BattleGameOverEvent)
        {
            _overView.SetActive(true);
        }
    }
}
