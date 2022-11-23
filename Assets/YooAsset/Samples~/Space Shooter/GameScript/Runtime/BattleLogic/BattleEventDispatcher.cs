using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BattleEventDispatcher
{
	public static void SendScoreChangeMsg(int currentScores)
	{
		BattleEventDefine.ScoreChange msg = new BattleEventDefine.ScoreChange();
		msg.CurrentScores = currentScores;
		EventManager.SendMessage(msg);
	}

	public static void SendGameOverMsg()
	{
		BattleEventDefine.GameOver msg = new BattleEventDefine.GameOver();
		EventManager.SendMessage(msg);
	}
}