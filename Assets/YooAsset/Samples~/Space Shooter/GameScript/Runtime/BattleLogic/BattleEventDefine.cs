using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEventDefine
{
	public class ScoreChange : IEventMessage
	{
		public int CurrentScores;
	}

	public class GameOver : IEventMessage
	{
	}
}