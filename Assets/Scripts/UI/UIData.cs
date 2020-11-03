using System;
using UnityEngine;

[CreateAssetMenu(menuName = "UIData")]
public class UIData : ScriptableObject
{
	public Sprite m_SkillsPortrait;
	public UIPanels m_Panels;
	public UIDialogue m_Dialogue;
	public UIBust m_Bust;
	public UISkillDiamonds m_SkillDiamonds;
	public UIIconColors m_IconColors;
}

[Serializable]
public class UIPanels
{
	public Sprite m_LeftPanel;
	public Sprite m_RightPanel;
	public Sprite m_RightPanelSmall;
}

[Serializable]
public class UIDialogue
{
	public Sprite m_BodyBox;
	public Sprite m_NameBox;
}

[Serializable]
public class UIBust
{
	public Sprite m_BustForeground;
	public Sprite m_BustBackground;
	public Sprite m_Healthbar;
	public Sprite m_PassiveBackground;
}

[Serializable]
public class UISkillDiamonds
{
	public Sprite m_SkillDiamondSides;
	public Sprite m_SkillDiamondCenter;
	public Color m_SkillCloudColor = Color.white;
}

[Serializable]
public class UIIconColors
{
	public Color m_IconLight = Color.white;
	public Color m_IconDark = Color.black;
}


