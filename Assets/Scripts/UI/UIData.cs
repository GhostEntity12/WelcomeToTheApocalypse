using UnityEngine;

[CreateAssetMenu(menuName = "UIData")]
public class UIData : ScriptableObject
{
	public Sprite m_SkillsPortrait;
	public UIPanels m_Panels;
	public UISkillDiamonds m_SkillDiamonds;
	public UIIconColors m_IconColors;
	public Sprite m_Healthbar;
}

[System.Serializable]
public class UIPanels
{
	public Sprite m_LeftPanel;
	public Sprite m_RightPanel;
	public Sprite m_RightPanelSmall;
}

[System.Serializable]
public class UISkillDiamonds
{
	public Sprite m_SkillDiamondSides;
	public Sprite m_SkillDiamondCenter;
	public Color m_SkillCloudColor = Color.white;
}

[System.Serializable]
public class UIIconColors
{
	public Color m_IconLight = Color.white;
	public Color m_IconDark = Color.black;
}


