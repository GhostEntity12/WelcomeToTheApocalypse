using System;
using UnityEngine;
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
		public RenderTexture m_PortraitRenderTexture;
		public Color m_Dark;
		public Color m_Medium;
		public Color m_Light;
		public Sprite m_SkillBg;
	}

	[Serializable]
	public class TweenedElement
	{
		public Transform m_Transform;
		internal Vector3[] m_Cache = new Vector3[2];
	}

	public bool m_Debug = true;
	public TextAsset m_TestDialogue;

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
	public Image[] m_SkillSlots;
	public RawImage m_PortraitRenderTexture;
	public Image m_LeftSpeakerImage;
	public Image m_RightSpeakerImage;


	[Header("Tweening")]
	public float m_TweenSpeed = 0.2f;
	[Space(10)]
	public TweenedElement m_PortraitUI;
	public TweenedElement m_SkillsUI;
	public TweenedElement m_DialogueUI;
	public TweenedElement m_RightSpeaker;
	public TweenedElement m_LeftSpeaker;

	public enum ScreenState { Onscreen, Offscreen }

	private void Awake()
	{
		m_Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		// Cache the positions
		SetCachesAndPosition(m_PortraitUI, new Vector3(-300, -300));
		SetCachesAndPosition(m_SkillsUI, new Vector3(300, -300));
		SetCachesAndPosition(m_DialogueUI, new Vector3(0, -600));
		SetCachesAndPosition(m_LeftSpeaker, new Vector3(-1400, 0));
		SetCachesAndPosition(m_RightSpeaker, new Vector3(1400, 0));
	}

	/// <summary>
	/// Caches the positions of an object for tweening
	/// </summary>
	/// <param name="tweenedElement">The element whose positions are to be cached</param>
	/// <param name="offset">The offset for when the element is offscreen</param>
	void SetCachesAndPosition(TweenedElement tweenedElement, Vector3 offset)
	{
		tweenedElement.m_Cache[0] = tweenedElement.m_Transform.position;
		tweenedElement.m_Cache[1] = tweenedElement.m_Cache[0] + offset;
		tweenedElement.m_Transform.position = tweenedElement.m_Cache[1];
	}

	/// <summary>
	/// Loads UI data
	/// </summary>
	/// <param name="uiData"></param>
	private void LoadUI(UIData uiData)
	{
		foreach (Image slot in m_SkillSlots)
		{
			slot.sprite = uiData.m_SkillBg;
		}

		if (uiData.m_PortraitRenderTexture)
		{
			m_PortraitRenderTexture.color = new Color(1, 1, 1, 1);
			m_PortraitImage.sprite = null;
			m_PortraitRenderTexture.texture = uiData.m_PortraitRenderTexture;
		}
		else
		{
			m_PortraitRenderTexture.texture = null;
			m_PortraitRenderTexture.color = new Color(1, 1, 1, 0);
			m_PortraitImage.sprite = uiData.m_SkillsPortrait;
		}
		m_FaceBackground.color = uiData.m_Medium;
		m_SkillsBackground.color = uiData.m_Light;
		m_TurnBackground.color = uiData.m_Dark;

		for (int i = 0; i < m_SkillSlots.Length; i++)
		{
			// TODO: Refactor
			m_SkillSlots[i].gameObject.SetActive(i < GameManager.m_Instance.GetSelectedUnit().GetSkills().Count);
		}
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
				LoadUI(m_DeathUIData);
				break;
			case UIStyle.Pestilence:
				LoadUI(m_PestilenceUIData);
				break;
			case UIStyle.Famine:
				LoadUI(m_FamineUIData);
				break;
			case UIStyle.War:
				LoadUI(m_WarUIData);
				break;
			case UIStyle.Enemy:
				LoadUI(m_EnemyUIData);
				break;
			default:
				break;
		}

		onComplete?.Invoke();
	}

	private void Update()
	{
		if (m_Debug)
		{
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				SlideSkills(ScreenState.Offscreen,
					() => LoadUI(UIStyle.Death,
						() => SlideSkills(ScreenState.Onscreen)));
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				SlideSkills(ScreenState.Offscreen,
					() => LoadUI(UIStyle.Pestilence,
						() => SlideSkills(ScreenState.Onscreen)));
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				SlideSkills(ScreenState.Offscreen,
					() => LoadUI(UIStyle.Famine,
						() => SlideSkills(ScreenState.Onscreen)));
			}
			if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				SlideSkills(ScreenState.Offscreen,
					() => LoadUI(UIStyle.War,
						() => SlideSkills(ScreenState.Onscreen)));
			}
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				SlideSkills(ScreenState.Offscreen,
					() => LoadUI(UIStyle.Enemy,
						() => SlideSkills(ScreenState.Onscreen)));
			}
			if (Input.GetKeyDown(KeyCode.Minus))
			{
				SwapToDialogue();
			}
			if (Input.GetKeyDown(KeyCode.Equals))
			{
				SwapFromDialogue();
			}
		}
	}

	public UIStyle GetUIStyle(Unit unit)
	{
		if (unit.GetAllegiance() == Allegiance.Enemy) return UIStyle.Enemy;

		switch (unit.name)
		{
			case "Death":
				return UIStyle.Death;
			case "Pestilence":
				return UIStyle.Pestilence;
			case "Famine":
				return UIStyle.Famine;
			case "War":
				return UIStyle.War;
			default:
				return UIStyle.Enemy;
		}
	}

	void SlideElement(TweenedElement element, ScreenState screenState, Action actionOnFinish = null)
	{
		LeanTween.move(element.m_Transform.gameObject, element.m_Cache[(int)screenState], m_TweenSpeed).setEase(LeanTweenType.easeInOutCubic).setOnComplete(actionOnFinish);
	}

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

	public void SwapToDialogue(TextAsset sourceFile = null)
	{
		SlideSkills(ScreenState.Offscreen,
			() => SlideElement(m_DialogueUI, ScreenState.Onscreen,
				() => DialogueManager.instance.TriggerDialogue(sourceFile ?? m_TestDialogue)));
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

	public void SwapDialogueSkin(UIStyle uiStyle)
	{
		SlideElement(m_DialogueUI, ScreenState.Offscreen,
			() => ChangeDialogueSkin(uiStyle,
				() => SlideElement(m_DialogueUI, ScreenState.Onscreen)));
	}

	void ChangeDialogueSkin(UIStyle uiStyle, Action actionOnFinish)
	{
		// TODO: implement skin change once UI is decided
		actionOnFinish();
	}
}
