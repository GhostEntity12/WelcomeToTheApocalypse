using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum UIStyle
{
	Death, Pestilence, Famine, War, Enemy
}

public class UIManager : MonoBehaviour
{
	public static UIManager m_Instance;

	[Serializable]
	public class UIData
	{
		public Sprite m_SkillsPortrait;
		//public RenderTexture m_PortraitRenderTexture;
		public Color m_Dark;
		public Color m_Medium;
		public Color m_Light;
		public Sprite m_SkillBg;
	}

	[Serializable]
	public class TweenedElement
	{
		public RectTransform m_RectTransform;
		internal Vector2[] m_Cache = new Vector2[2];
	}

	[Header("Data")]
	public UIData m_DeathUIData;
	public UIData m_PestilenceUIData;
	public UIData m_FamineUIData;
	public UIData m_WarUIData;
	public UIData m_EnemyUIData;

	[Header("Graphical Elements")]
	public Image m_FaceBackground;
	public Image m_SkillsBackground;
	public Image m_TurnBackground;
	public Image m_PortraitImage;
	public SkillButton[] m_SkillSlots;
	public RawImage m_PortraitRenderTexture;
	public Image m_LeftSpeakerImage;
	public Image m_RightSpeakerImage;


	[Header("Tweening")]
	public float m_TweenSpeed = 0.2f;
	[Space(10)]
	public TweenedElement m_PortraitUI;
	public TweenedElement m_SkillsUI;
	public TweenedElement m_LeftSpeaker;
	public TweenedElement m_RightSpeaker;
	public TweenedElement m_DialogueUI;

	public enum ScreenState { Onscreen, Offscreen }

	private void Awake()
	{
		m_Instance = this;

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
		// Cache the positions
		SetCachesAndPosition(m_PortraitUI, new Vector2(-400, -400));
		SetCachesAndPosition(m_SkillsUI, new Vector2(400, -400));
		SetCachesAndPosition(m_LeftSpeaker, new Vector2(-800, 0));
		SetCachesAndPosition(m_RightSpeaker, new Vector2(800, 0));
		SetCachesAndPosition(m_DialogueUI, new Vector2(0, -400));
	}

	/// <summary>
	/// Caches the positions of an object for tweening
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
	/// Loads a skin for the skills UI
	/// </summary>
	/// <param name="uiData"></param>
	private void LoadSkillsSkin(UIData uiData)
	{
		foreach (SkillButton slot in m_SkillSlots)
		{
			slot.m_Image.sprite = uiData.m_SkillBg;
		}

		m_PortraitImage.sprite = uiData.m_SkillsPortrait;
		m_FaceBackground.color = uiData.m_Medium;
		m_SkillsBackground.color = uiData.m_Light;
		m_TurnBackground.color = uiData.m_Dark;

		for (int i = 0; i < m_SkillSlots.Length; i++)
		{
			// TODO: Refactor
			m_SkillSlots[i].gameObject.SetActive(i < GameManager.m_Instance.GetSelectedUnit().GetSkills().Count);
			m_SkillSlots[i].m_Skill = GameManager.m_Instance.GetSelectedUnit().GetSkill(i);
			m_SkillSlots[i].UpdateTooltip();
		}
	}

	/// <summary>
	/// Loads a skin for the dialogue UI
	/// </summary>
	/// <param name="uiStyle"></param>
	/// <param name="actionOnFinish"></param>
	private void LoadDialogueSkin(UIStyle uiStyle, Action actionOnFinish)
	{
		// TODO: implement skin change once UI is decided
		DialogueManager.instance.StartDisplaying();
		actionOnFinish();
	}

	/// <summary>
	/// Loads a UI skin
	/// </summary>
	/// <param name="skin">The skin to load</param>
	public void LoadUI(UIStyle skin, Action onComplete = null)
	{
		switch (skin)
		{
			case UIStyle.Death:
				LoadSkillsSkin(m_DeathUIData);
				break;
			case UIStyle.Pestilence:
				LoadSkillsSkin(m_PestilenceUIData);
				break;
			case UIStyle.Famine:
				LoadSkillsSkin(m_FamineUIData);
				break;
			case UIStyle.War:
				LoadSkillsSkin(m_WarUIData);
				break;
			case UIStyle.Enemy:
				LoadSkillsSkin(m_EnemyUIData);
				break;
			default:
				break;
		}

		onComplete?.Invoke();
	}

	/// <summary>
	/// Gets the appropriate UI style of a unit
	/// </summary>
	/// <param name="unit"></param>
	/// <returns></returns>
	public UIStyle GetUIStyle(Unit unit)
	{
		if (unit.GetAllegiance() == Allegiance.Enemy) return UIStyle.Enemy;

		return GetUIStyle(unit.name);
	}

	/// <summary>
	/// Gets the appropriate UI style by a name
	/// </summary>
	/// <param name="unit"></param>
	/// <returns></returns>
	public UIStyle GetUIStyle(string unitName)
	{
		switch (unitName.ToLower())
		{
			case string s when s.ToLower().Contains("death"):
				return UIStyle.Death;
			case string s when s.ToLower().Contains("pestilence"):
				return UIStyle.Pestilence;
			case string s when s.ToLower().Contains("famine"):
				return UIStyle.Famine;
			case string s when s.ToLower().Contains("war"):
				return UIStyle.War;
			default:
				Debug.LogWarning($"No character with name {unitName} found.");
				return UIStyle.Enemy;
		}
	}

	/// <summary>
	/// Abstracted function which allows sliding UI elements on or offscreen if they are defined as TweenedElements
	/// </summary>
	/// <param name="element">The element to be tweened</param>
	/// <param name="screenState">Whether the object should be on or off screen at the end of the tween</param>
	/// <param name="actionOnFinish">Function on callback</param>
	/// <param name="tweenType">Overides the twwn type</param>
	public void SlideElement(TweenedElement element, ScreenState screenState, Action actionOnFinish = null, LeanTweenType tweenType = LeanTweenType.easeInOutCubic)
	{
		LeanTween.move(element.m_RectTransform, element.m_Cache[(int)screenState], m_TweenSpeed).setEase(tweenType).setOnComplete(actionOnFinish);
	}

	/// <summary>
	/// Shorthand way of sliding both parts of the skills UI
	/// </summary>
	/// <param name="screenState"></param>
	/// <param name="actionOnFinish"></param>
	public void SlideSkills(ScreenState screenState, Action actionOnFinish = null)
	{
		SlideElement(m_PortraitUI, screenState, actionOnFinish);
		SlideElement(m_SkillsUI, screenState);
	}

	public void SwapUI(UIStyle uiStyle)
	{
		SlideSkills(ScreenState.Offscreen,
			() => LoadUI(uiStyle,
				() => SlideSkills(ScreenState.Onscreen)));
	}

	public void SwapDialogue(UIStyle uiStyle)
	{
		SlideElement(m_DialogueUI, ScreenState.Offscreen,
			() => LoadDialogueSkin(uiStyle,
				() => SlideElement(m_DialogueUI, ScreenState.Onscreen)));
	}

	public void SwapToDialogue(TextAsset sourceFile)
	{
		if (sourceFile)
		{
			SlideSkills(ScreenState.Offscreen,
				() => DialogueManager.instance.TriggerDialogue(sourceFile));
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
}
