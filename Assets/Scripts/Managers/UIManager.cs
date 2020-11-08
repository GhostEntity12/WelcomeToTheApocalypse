using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public enum ScreenState { Onscreen, Offscreen }

	public static UIManager m_Instance;
	public CanvasGroup m_BlackScreen;

	/// <summary>
	/// List of UI elements that block the player from being able to interact with the game.
	/// </summary>
	private List<InputBlockingUI> m_InputBlockingUIElements = new List<InputBlockingUI>();

	[Serializable]
	public class TweenedElement
	{
		public RectTransform m_RectTransform;
		internal Vector2[] m_Cache = new Vector2[2];
	}

	public bool m_ActiveUI = false;

	[Header("Skin Data")]
	public Color[] m_TurnIndicatorColors = new Color[2];

	[Header("Graphical Elements")]
	public Image m_FaceBackground;
	public Image m_SkillsBackground;
	public Image m_SkillsBackgroundSmall;
	public Image m_PortraitImage;
	public Image m_PortraitBackground;
	public Image m_PortraitForeground;
	public SkillButton[] m_SkillSlots;
	public Image m_LeftSpeakerImage;
	public Image m_RightSpeakerImage;
	public Image m_TurnIndicatorImage;
	/// <summary>
	/// The button for ending the turn.
	/// </summary>
	public EndTurnButton m_EndTurnButton = null;
	public UIHealthBar m_UIHealthBar = null;
	public Image m_DialogueBody;
	public Image m_DialogueName;


	[Header("Tweening")]
	public float m_TweenSpeed = 0.2f;
	[Space(5)]
	public TweenedElement m_PortraitUI;
	public TweenedElement m_SkillsUI;
	public TweenedElement m_LeftSpeaker;
	public TweenedElement m_RightSpeaker;
	public TweenedElement m_DialogueUI;
	public TweenedElement m_TurnIndicatorUI;

	[Header("Other Elements")]
	/// <summary>
	/// The turn indicator.
	/// </summary>
	public TurnIndicator m_TurnIndicator = null;
	/// <summary>
	/// The action point counter, for the currently selected unit.
	/// </summary>
	public ActionPointCounter m_ActionPointCounter = null;

	[Header("Additional Screens")]
	public PrematureTurnEndDisplay m_PrematureTurnEndScreen = null;
	private InputBlockingUI m_EndTurnBlocker;
	public GameObject m_PauseScreen = null;
	private bool m_Paused = false;
	public TutorialPanner m_Tutorial;

	public CrawlDisplay m_CrawlDisplay;

	private void Awake()
	{
		m_Instance = this;

		m_InputBlockingUIElements = FindObjectsOfType<InputBlockingUI>().ToList();
		m_CrawlDisplay = GetComponent<CrawlDisplay>();
		m_EndTurnBlocker = m_PrematureTurnEndScreen.GetComponent<InputBlockingUI>();

		// Creates an EventSystem if it can't find one
		if (FindObjectOfType<EventSystem>() == null)
		{
			GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
			eventSystem.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
		}
	}

	/// <summary>
	/// Start is called before the first frame update
	/// </summary>
	private void Start()
	{
		m_EndTurnButton.UpdateCurrentTeamTurn(GameManager.m_Instance.m_TeamCurrentTurn);
		m_TurnIndicator.UpdateTurnIndicator(GameManager.m_Instance.m_TeamCurrentTurn);

		// Cache the positions
		SetCachesAndPosition(m_PortraitUI, new Vector2(-400, -400));
		SetCachesAndPosition(m_SkillsUI, new Vector2(400, -400));
		SetCachesAndPosition(m_LeftSpeaker, new Vector2(-800, 0));
		SetCachesAndPosition(m_RightSpeaker, new Vector2(800, 0));
		SetCachesAndPosition(m_DialogueUI, new Vector2(0, -400));
		SetCachesAndPosition(m_TurnIndicatorUI, new Vector2(300, 300));
	}

	private void Update()
	{
		if (DialogueManager.instance.dialogueActive)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (m_PrematureTurnEndScreen.m_Active)
			{
				m_PrematureTurnEndScreen.DisplayPrematureEndScreen(false);
			}
			else
			{
				TogglePause();
			}
		}
	}

	public void TogglePause()
	{
		LeanTween.scale(m_PauseScreen.gameObject, m_Paused ? Vector2.zero : Vector2.one, 0.03f).setEaseInOutCubic();
		m_Paused = !m_Paused;
		m_ActiveUI = m_Paused;
	}

	public InputBlockingUI EndTurnBlocker() => m_EndTurnBlocker;

	/// <summary>
	/// Caches the positions of an TweenedElement object for tweening
	/// </summary>
	/// <param name="tweenedElement">The element whose positions are to be cached</param>
	/// <param name="offset">The offset for when the element is offscreen</param>
	private void SetCachesAndPosition(TweenedElement tweenedElement, Vector2 offset)
	{
		tweenedElement.m_Cache[0] = tweenedElement.m_RectTransform.anchoredPosition;
		tweenedElement.m_Cache[1] = tweenedElement.m_Cache[0] + offset;
		tweenedElement.m_RectTransform.anchoredPosition = tweenedElement.m_Cache[1];
	}

	/// <summary>
	/// Abstracted function which allows sliding UI elements on or offscreen if they are defined as TweenedElements
	/// </summary>
	/// <param name="element">The element to be tweened</param>
	/// <param name="screenState">Whether the object should be on or off screen at the end of the tween</param>
	/// <param name="onComplete">Function on callback</param>
	/// <param name="tweenType">Overides the twwn type</param>
	public void SlideElement(TweenedElement element, ScreenState screenState, Action onComplete = null, LeanTweenType tweenType = LeanTweenType.easeInOutCubic)
	{
		LeanTween.move(element.m_RectTransform, element.m_Cache[(int)screenState], m_TweenSpeed).setEase(tweenType).setOnComplete(onComplete);
	}

	#region SkillsTweening
	/// <summary>
	/// Loads a skin for the skills UI
	/// </summary>
	/// <param name="uiData"></param>
	private void LoadSkillsSkin(UIData uiData, Action onComplete = null)
	{
		m_FaceBackground.sprite = uiData.m_Panels.m_LeftPanel;
		m_SkillsBackground.sprite = uiData.m_Panels.m_RightPanel;
		m_SkillsBackgroundSmall.sprite = uiData.m_Panels.m_RightPanelSmall;
		m_PortraitImage.sprite = uiData.m_SkillsPortrait;
		m_PortraitBackground.sprite = uiData.m_Bust.m_BustBackground;
		m_PortraitForeground.sprite = uiData.m_Bust.m_BustForeground;
		m_UIHealthBar.m_HealthBarBackground.sprite = uiData.m_Bust.m_Healthbar;

		// Assign the skills
		for (int i = 0; i < m_SkillSlots.Length; i++)
		{
			SkillButton slot = m_SkillSlots[i];
			m_SkillSlots[i].transform.parent.gameObject.SetActive(i < GameManager.m_Instance.GetSelectedUnit().GetSkills().Count);
			slot.m_Skill = GameManager.m_Instance.GetSelectedUnit().GetSkill(i);
			if (slot.m_Skill)
			{
				slot.m_LightIcon.sprite = slot.m_Skill.m_LightIcon;
				slot.m_LightIcon.color = uiData.m_IconColors.m_IconLight;
				slot.m_DarkIcon.sprite = slot.m_Skill.m_DarkIcon;
				slot.m_DarkIcon.color = uiData.m_IconColors.m_IconDark;
			}
			slot.UpdateTooltip();
		}


		// Update the skin and cooldown of the skills
		foreach (SkillButton slot in m_SkillSlots)
		{
			slot.m_SidesImage.sprite = uiData.m_SkillDiamonds.m_SkillDiamondSides;
			slot.m_CenterImage.sprite = uiData.m_SkillDiamonds.m_SkillDiamondCenter;
			slot.m_LightningImage.material.SetColor("_UICloudTint", uiData.m_SkillDiamonds.m_SkillCloudColor);
			slot.UpdateCooldownDisplay();
		}

		onComplete?.Invoke();
	}

	/// <summary>
	/// Shorthand way of sliding both parts of the skills UI
	/// </summary>
	/// <param name="screenState"></param>
	/// <param name="actionOnFinish"></param>
	public void SlideSkills(ScreenState screenState, Action onComplete = null)
	{
		SlideElement(m_PortraitUI, screenState, onComplete);
		SlideElement(m_SkillsUI, screenState);
	}

	public void SwapSkillsUI(UIData uiStyle)
	{
		SlideSkills(ScreenState.Offscreen,
			() => LoadSkillsSkin(uiStyle,
				() => SlideSkills(ScreenState.Onscreen)));
	}
	#endregion

	#region DialogueTweening
	/// <summary>
	/// Loads a skin for the dialogue UI
	/// </summary>
	/// <param name="uiStyle"></param>
	/// <param name="actionOnFinish"></param>
	private void LoadDialogueSkin(UIData uiData, Action onComplete = null)
	{
		m_DialogueBody.sprite = uiData.m_Dialogue.m_BodyBox;
		m_DialogueName.sprite = uiData.m_Dialogue.m_NameBox;
		DialogueManager.instance.StartDisplaying();
		onComplete?.Invoke();
	}

	public void SwapToDialogue(TextAsset sourceFile, Action onDialogueEndAction = null)
	{
		if (sourceFile)
		{
			SlideSkills(ScreenState.Offscreen,
				() => DialogueManager.instance.QueueDialogue(sourceFile, onDialogueEndAction));
		}
		else
		{
			Debug.LogError("No dialogue file!");
		}
	}

	public void SwapFromDialogue()
	{
		SlideElement(m_DialogueUI, ScreenState.Offscreen, () =>
		{
			if (GameManager.m_Instance.GetSelectedUnit())
			{
				SlideSkills(ScreenState.Onscreen);
			}
		});
	}

	/// <summary>
	/// Swaps the style of the dialogue
	/// </summary>
	/// <param name="uiData"></param>
	public void SwapDialogue(UIData uiData)
	{
		SlideElement(m_DialogueUI, ScreenState.Offscreen,
			() => LoadDialogueSkin(uiData,
				() => SlideElement(m_DialogueUI, ScreenState.Onscreen)));
	}
	#endregion

	#region TurnIndicatorTweening
	public void SwapTurnIndicator(Allegiance newTeamTurn)
	{
		HideTurnIndicator(() =>
		{
			m_TurnIndicator.UpdateTurnIndicator(newTeamTurn);
			m_TurnIndicatorImage.color = m_TurnIndicatorColors[(int)newTeamTurn];
			ShowTurnIndicator();
		});
	}

	public void HideTurnIndicator(Action onComplete = null)
	{
		SlideElement(m_TurnIndicatorUI, ScreenState.Offscreen, onComplete);
	}

	public void ShowTurnIndicator(Action onComplete = null)
	{
		SlideElement(m_TurnIndicatorUI, ScreenState.Onscreen, onComplete);
	}
	#endregion

	public bool IsPrematureTurnEnding()
	{
		List<Unit> playerUnits = UnitsManager.m_Instance.m_PlayerUnits;

		foreach (Unit u in playerUnits)
		{
			if (u.GetCurrentMovement() > 0)
			{
				return true;
			}
			else if (u.GetActionPoints() > 0 && UnitsManager.m_Instance.m_ActiveEnemyUnits.Count != 0)
			{
				return true;
			}
		}

		return false;
	}

	public bool CheckUIBlocking()
	{
		// Check if the player's cursor is over any UI elements deemed to block the player's mouse inputs in the game world.
		foreach (InputBlockingUI iBUI in m_InputBlockingUIElements)
		{
			// If the mouse is over one of them, make note of it and break from the loop.
			// If the mouse is over a single element, no need to keep going through.
			if (iBUI.GetMouseOverUIElement())
			{
				return true;
			}
		}
		return false;
	}

	public void ShowCrawlButtons()
	{
		LeanTween.move(m_CrawlDisplay.m_CrawlButtons, Vector3.zero, 2f);
	}

	public void LoadSceneIndex(int index) => SceneManager.LoadScene(index);
}
