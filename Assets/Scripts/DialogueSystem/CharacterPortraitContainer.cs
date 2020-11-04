using UnityEngine;

[CreateAssetMenu]
public class CharacterPortraitContainer : ScriptableObject
{
	public UIData m_UiData;

	[Space(40)]
	public Sprite neutral;
	public Sprite angry;
	public Sprite happy;
	public Sprite tired;
}
