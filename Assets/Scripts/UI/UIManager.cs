using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIStyle
{
	Death, Pestilence, Famine, War, Enemy
}

public class UIManager : MonoBehaviour
{
	[System.Serializable]
	public class UIData
	{
		public Sprite m_Portrait;
		public Color m_Dark;
		public Color m_Medium;
		public Color m_Light;
		public Sprite m_SkillBg;
	}

	[Header("Debug")]
	public UIStyle DEBUGStyle;

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

	void OnValidate() => LoadUI(DEBUGStyle);

	void LoadUI(UIData uiData)
	{
		foreach (Image slot in m_SkillSlots)
		{
			slot.sprite = uiData.m_SkillBg;
		}

		m_Portrait.sprite = uiData.m_Portrait;
		m_FaceBackground.color = uiData.m_Medium;
		m_SkillsBackground.color = uiData.m_Light;
		m_TurnBackground.color = uiData.m_Dark;
	}

	// Loads a UI Style
	public void LoadUI(UIStyle style)
	{
		switch (style)
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
	}
}
