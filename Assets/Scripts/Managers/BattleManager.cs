using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum TargetingState
{
	// Player is currently selecting a node for movement.
	Move,

	// Player is currently selecting a node for using a skill.
	Skill,

	// Player isn't selecting a node for anything. (just to be safe)
	None
}

public class BattleManager : MonoBehaviour
{
	/// <summary>
	/// Instance of the game manager.
	///</summary>
	public static BattleManager m_Instance = null;

	/// <summary>
	/// Which team's turn is it currently.
	/// </summary>
	private Allegiance m_TeamCurrentTurn = Allegiance.Player;

	private PlayerManager m_PlayerManager;

	private AIManager m_AIManager;

	// [FMODUnity.EventRef]
	// public string m_TurnEndSound = "";
	
	[SerializeField]
	private TextAsset m_FailScript;
	[SerializeField]
	private TextAsset m_WinScript;

	// On startup.
	private void Awake()
	{
		m_Instance = this;

		CreateVersionText();

		if (!FindObjectOfType<MusicManager>())
		{
			GameObject musicManager = new GameObject("MusicManager", typeof(MusicManager));
			musicManager.GetComponent<MusicManager>().m_MusicEvent = "event:/Music";
		}
	}

	void Start()
	{
		m_PlayerManager = PlayerManager.m_Instance;
		m_AIManager = AIManager.m_Instance;
	}

	public void BattleUpdate()
	{
		switch(m_TeamCurrentTurn)
		{
			case Allegiance.Player:
			m_PlayerManager.PlayerBattleUpdate();
			break;

			case Allegiance.Enemy:
			m_AIManager.TakeAITurn();
			break;
		};
	}

	public bool TryEndTurn()
	{
		// Check player units for prematurely ending turn here.
		if (UIManager.m_Instance.IsPrematureTurnEnding())
		{
			UIManager.m_Instance.m_PrematureTurnEndScreen.DisplayPrematureEndScreen(true);
			return false;
		}

		EndCurrentTurn();
		return true;
	}

	/// <summary>
	/// End the current turn.
	/// </summary>
	public void EndCurrentTurn()
	{
		if (m_TeamCurrentTurn == Allegiance.Enemy)
			m_TeamCurrentTurn = Allegiance.Player;
		else if (m_TeamCurrentTurn == Allegiance.Player)
			m_TeamCurrentTurn = Allegiance.Enemy;

		Debug.Log($"============{m_TeamCurrentTurn} turn============");

		UIManager.m_Instance.SwapTurnIndicator(m_TeamCurrentTurn);

		UIManager.m_Instance.SlideSkills(UIManager.ScreenState.Offscreen);

		// Play the end turn sound on the camera.
		//FMODUnity.RuntimeManager.PlayOneShot(m_TurnEndSound, Camera.main.transform.position);

		foreach (Unit unit in m_TeamCurrentTurn == Allegiance.Player ? UnitsManager.m_Instance.m_PlayerUnits : UnitsManager.m_Instance.m_ActiveEnemyUnits)
		{
			unit.SetDealExtraDamage(0);
			unit.ResetActionPoints();
			unit.ResetCurrentMovement();

			// Check the passives of all the player units for any that trigger at the start of their turn.
			PassiveSkill ps = unit.GetPassiveSkill();
			if (ps)
			{
				if (ps.CheckPrecondition(TriggerType.OnTurnStart))
					ps.TakeEffect(unit);
			}


			// Reduce cooldowns
			foreach (BaseSkill s in unit.GetSkills())
			{
				s.DecrementCooldown();
			}

			// Deal with inflicted statuses
			// So using .ToList() creates a compy of the list to iterate through
			// but continues romoving from the source list. Not particularly
			// efficient for large lists, but easy enough here.
			foreach (InflictableStatus status in unit.GetInflictableStatuses().ToList())
			{
				// If returns true, status effect's duration has reached 0, remove the status effect.
				if (status.DecrementDuration())
				{
					unit.RemoveStatusEffect(status);
				}
				// Otherwise do the effect
				else if (status.CheckPrecondition(TriggerType.OnTurnStart) == true)
				{
					status.TakeEffect(unit);
				}
			}
		}
	}


	/// <summary>
	/// Check the player's units to see if they're alive.
	/// </summary>
	public bool CheckIfAnyPlayerUnitsAlive()
	{
		// If true, all player units are dead.
		if (UnitsManager.m_Instance.m_PlayerUnits.Where(u => u.GetAlive()).Count() == 0)
		{
			// All the player's units are dead, the player lost.
			// Pause the game and display the lose screen for the player.
			Debug.Log("Everybody's dead, everybody's dead Dave!");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Get the allegiance of which team's turn it currently is.
	/// </summary>
	/// <returns>The allegiance of the team whose turn it currently is.</returns>
	public Allegiance GetCurrentTurn() { return m_TeamCurrentTurn; }

	public static void CreateVersionText()
	{
		if (GameObject.Find("VersionCanvas")) return;
		if (Application.version.Contains("Gold")) return;
		if (Application.version.Contains("RC")) return;

		GameObject cgo = new GameObject("VersionCanvas", typeof(Canvas), typeof(CanvasScaler));
		DontDestroyOnLoad(cgo);
		CanvasScaler cs = cgo.gameObject.GetComponent<CanvasScaler>();
		Canvas c = cgo.GetComponent<Canvas>();
		c.renderMode = RenderMode.ScreenSpaceOverlay;
		cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		cs.referenceResolution = new Vector2(1920, 1080);

		GameObject g = new GameObject("Version", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		RectTransform rt = g.GetComponent<RectTransform>();
		TextMeshProUGUI versionText = g.GetComponent<TextMeshProUGUI>();
		g.transform.SetParent(c.transform);
		g.transform.localScale = Vector3.one;
		rt.pivot = Vector2.zero;
		rt.anchorMax = Vector2.zero;
		rt.anchorMin = Vector2.zero;
		rt.position = Vector3.zero;
		versionText.autoSizeTextContainer = true;
		versionText.text = Application.version;
	}

	public void LoadMainMenu()
	{
		MusicManager.m_Instance.SetHorsemen(0);
		SceneManager.LoadScene(0);
	}

	public TextAsset GetWinScript()
	{
		return m_WinScript;
	}

	public TextAsset GetFailScript()
	{
		return m_FailScript;
	}
}
