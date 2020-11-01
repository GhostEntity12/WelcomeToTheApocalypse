using UnityEngine;

[CreateAssetMenu(menuName = "UIData")]
public class UIData : ScriptableObject
{
	public Sprite m_SkillsPortrait;
	public Color m_Dark = Color.white;
	public Color m_Medium = Color.white;
	public Color m_Light = Color.white;
	public Sprite m_SkillBg;

	public Color m_IconLight = Color.white;
	public Color m_IconDark = Color.white;
}
