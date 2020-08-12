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

	[System.Serializable]
	public class UIData
	{
		public Sprite m_Portrait;
		public Color m_Dark;
		public Color m_Medium;
		public Color m_Light;
		public Sprite m_SkillBg;
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
	public Image m_Portrait;
	public Image[] m_SkillSlots;


	[Header("Tweening")]
	public float m_TweenSpeed = 0.2f;
	[Space(10)]
	public GameObject m_PortraitUI;
	public GameObject m_SkillsUI;
	public GameObject m_DialogueUI;


	private Vector3 m_InCachePortrait;
	private Vector3 m_InCacheSkills;
	private Vector3 m_InCacheDialogue;
	private Vector3 m_OutCachePortrait;
	private Vector3 m_OutCacheSkills;
	private Vector3 m_OutCacheDialogue;

	private void Awake()
	{
		m_Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{
		m_InCachePortrait = m_PortraitUI.transform.position;
		m_OutCachePortrait = m_InCachePortrait + new Vector3(-150, -150);
		m_PortraitUI.transform.position = m_OutCachePortrait;

		m_InCacheSkills = m_SkillsUI.transform.position;
		m_OutCacheSkills = m_InCacheSkills + new Vector3(150, -150);
		m_SkillsUI.transform.position = m_OutCacheSkills;

		m_InCacheDialogue = m_DialogueUI.transform.position;
		m_OutCacheDialogue = m_InCacheDialogue + new Vector3(0, -400);
		m_DialogueUI.transform.position = m_OutCacheDialogue;

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

		m_Portrait.sprite = uiData.m_Portrait;
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
				SlideSkillsOut(() => LoadUI(UIStyle.Death, () => SlideSkillsIn()));
			}
			if (Input.GetKeyDown(KeyCode.Alpha7))
			{
				SlideSkillsOut(() => LoadUI(UIStyle.Pestilence, () => SlideSkillsIn()));
			}
			if (Input.GetKeyDown(KeyCode.Alpha8))
			{
				SlideSkillsOut(() => LoadUI(UIStyle.Famine, () => SlideSkillsIn()));
			}
			if (Input.GetKeyDown(KeyCode.Alpha9))
			{
				SlideSkillsOut(() => LoadUI(UIStyle.War, () => SlideSkillsIn()));
			}
			if (Input.GetKeyDown(KeyCode.Alpha0))
			{
				SlideSkillsOut(() => LoadUI(UIStyle.Enemy, () => SlideSkillsIn()));
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

	public void SlideSkillsOut(Action actionOnFinish = null)
	{
		LeanTween.move(m_PortraitUI, m_OutCachePortrait, m_TweenSpeed).setEase(LeanTweenType.easeInOutCubic).setOnComplete(actionOnFinish);
		LeanTween.move(m_SkillsUI, m_OutCacheSkills, m_TweenSpeed).setEase(LeanTweenType.easeInOutCubic);
	}

	void SlideSkillsIn(Action actionOnFinish = null)
	{
		LeanTween.move(m_PortraitUI, m_InCachePortrait, m_TweenSpeed).setEase(LeanTweenType.easeInOutCubic).setOnComplete(actionOnFinish);
		LeanTween.move(m_SkillsUI, m_InCacheSkills, m_TweenSpeed).setEase(LeanTweenType.easeInOutCubic);
	}

	void SlideDialogueIn(Action actionOnFinish = null)
	{
		LeanTween.move(m_DialogueUI, m_InCacheDialogue, m_TweenSpeed).setEase(LeanTweenType.easeInOutCubic).setOnComplete(actionOnFinish);
	}

	void SlideDialogueOut(Action actionOnFinish = null)
	{
		LeanTween.move(m_DialogueUI, m_OutCacheDialogue, m_TweenSpeed).setEase(LeanTweenType.easeInOutCubic).setOnComplete(actionOnFinish);
	}

	public void SwapUI(UIStyle uiStyle)
	{
		SlideSkillsOut(() => LoadUI(uiStyle, () => SlideSkillsIn()));
	}

	public void SwapToDialogue(TextAsset sourceFile = null)
	{
		SlideSkillsOut(() => SlideDialogueIn(() => DialogueManager.instance.TriggerDialogue(sourceFile ?? m_TestDialogue)));
	}

	public void SwapFromDialogue()
	{
		SlideDialogueOut(() => SlideSkillsIn());
	}
}
