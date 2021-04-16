using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
	Roam,
	Battle,
	Count
}

public class GameManager : MonoBehaviour
{
	public static GameManager m_Instance;

	private BattleManager m_BattleManager = null;

	private RoamManager m_RoamManager = null;

	private GameState m_GameState = GameState.Roam;

	[SerializeField]
	private GameState m_StartingGameState;

	void Awake()
	{
		m_Instance = this;
		m_GameState = m_StartingGameState;
	}

	void Start()
	{
		m_BattleManager = BattleManager.m_Instance;
		m_RoamManager = RoamManager.m_Instance;
	}

    // Update is called once per frame
    void Update()
    {
		switch(m_GameState)
		{
			case GameState.Battle:
			m_BattleManager.BattleUpdate();
			break;
			
			case GameState.Roam:
			m_RoamManager.RoamUpdate();
			break;
		};
    }

	public void SetGameState(GameState state)
	{
		m_GameState = state;
	}

	public GameState GetGameState()
	{
		return m_GameState;
	}
}
